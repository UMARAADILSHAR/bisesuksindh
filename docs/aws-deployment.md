# AWS Deployment Guide — BISE Hyderabad Portal

This guide walks you through deploying the BISE Hyderabad Blazor Server application to AWS using **ECS Fargate**, **Amazon RDS for SQL Server**, and **GitHub Actions** CI/CD.

---

## Architecture

```
Internet → ALB (port 80/443) → ECS Fargate (port 8080) → RDS SQL Server (port 1433)
                                         ↓
                                  CloudWatch Logs
```

| Component | AWS Service | Details |
|---|---|---|
| Container Hosting | ECS Fargate | Serverless, no EC2 management |
| Container Registry | Amazon ECR | Private Docker image storage |
| Load Balancer | Application Load Balancer | Sticky sessions for Blazor SignalR |
| Database | Amazon RDS SQL Server Express | Managed, encrypted, automated backups |
| Secrets | AWS Secrets Manager | DB credentials, seed password |
| Logging | CloudWatch Logs | 30-day retention, JSON structured |
| CI/CD | GitHub Actions | OIDC auth, zero stored credentials |
| Infrastructure | CloudFormation | Single-stack IaC |

---

## Prerequisites

1. **AWS CLI** installed and configured
2. **AWS Account** with permissions to create IAM, VPC, ECS, RDS, ECR, ALB resources
3. **GitHub Repository** with the BISE Hyderabad source code
4. **.NET 10 SDK** (for local builds/testing)

---

## Step 1: Deploy the CloudFormation Stack

```powershell
# Validate the template first
aws cloudformation validate-template --template-body file://aws/cloudformation.yml

# Create the stack (replace parameters with your values)
aws cloudformation create-stack `
    --stack-name bise-hyderabad-production `
    --template-body file://aws/cloudformation.yml `
    --capabilities CAPABILITY_NAMED_IAM `
    --parameters `
        ParameterKey=DBMasterPassword,ParameterValue=YourSecureDBPassword123! `
        ParameterKey=SeedPassword,ParameterValue=YourSeedPassword123!

# Wait for stack creation to complete (takes ~15-20 minutes due to RDS)
aws cloudformation wait stack-create-complete --stack-name bise-hyderabad-production

# View outputs (ALB URL, ECR URI, etc.)
aws cloudformation describe-stacks `
    --stack-name bise-hyderabad-production `
    --query "Stacks[0].Outputs" `
    --output table
```

> **Important**: Save the stack outputs — you'll need the **ECR Repository URI** and **GitHub Actions Role ARN**.

---

## Step 2: Configure GitHub Secrets

Go to your GitHub repository → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**:

| Secret Name | Value | Example |
|---|---|---|
| `AWS_ACCOUNT_ID` | Your 12-digit AWS account ID | `123456789012` |
| `AWS_REGION` | AWS region where the stack was deployed | `us-east-1` |

> **Note**: No AWS access keys are needed! The pipeline uses OIDC federation via the IAM role created by CloudFormation.

---

## Step 3: First Deployment (Push Initial Image)

The ECS service needs an initial Docker image in ECR to start. Build and push manually for the first time:

```powershell
# Get your ECR URI from the stack outputs
$ECR_URI = (aws cloudformation describe-stacks `
    --stack-name bise-hyderabad-production `
    --query "Stacks[0].Outputs[?OutputKey=='ECRRepositoryUri'].OutputValue" `
    --output text)

$AWS_REGION = "us-east-1"  # Replace with your region

# Authenticate Docker to ECR
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI.Split('/')[0]

# Build and push
docker build -t ${ECR_URI}:latest .
docker push ${ECR_URI}:latest

# Force ECS to pick up the new image
aws ecs update-service `
    --cluster bise-hyderabad-production `
    --service bise-hyderabad-production-svc `
    --force-new-deployment
```

---

## Step 4: Verify Deployment

```powershell
# Get the ALB URL
$ALB_URL = (aws cloudformation describe-stacks `
    --stack-name bise-hyderabad-production `
    --query "Stacks[0].Outputs[?OutputKey=='ALBUrl'].OutputValue" `
    --output text)

Write-Host "Application URL: $ALB_URL"

# Check health endpoints
Invoke-RestMethod "$ALB_URL/health"   # Liveness
Invoke-RestMethod "$ALB_URL/ready"    # Readiness (DB check)

# View ECS service status
aws ecs describe-services `
    --cluster bise-hyderabad-production `
    --services bise-hyderabad-production-svc `
    --query "services[0].{Status:status,Running:runningCount,Desired:desiredCount}"
```

---

## Ongoing Deployments

After the first deployment, all subsequent deployments are **automatic**:

1. Push code to the `main` branch
2. GitHub Actions runs tests → builds Docker image → pushes to ECR → deploys to ECS
3. ECS performs a rolling deployment (zero downtime)
4. Pipeline waits for service stability before marking as successful

---

## Monitoring & Troubleshooting

### View Container Logs

```powershell
# Stream live logs
aws logs tail /ecs/bise-hyderabad-production --follow

# Search logs for errors
aws logs filter-log-events `
    --log-group-name /ecs/bise-hyderabad-production `
    --filter-pattern "ERROR"
```

### Check ECS Task Status

```powershell
# List running tasks
aws ecs list-tasks --cluster bise-hyderabad-production --service-name bise-hyderabad-production-svc

# Describe a task (get IP, status, health)
aws ecs describe-tasks `
    --cluster bise-hyderabad-production `
    --tasks <task-arn>
```

### Common Issues

| Issue | Cause | Fix |
|---|---|---|
| Tasks keep restarting | Database connection failure | Check RDS security group allows ECS SG on port 1433 |
| Health check failing | App not ready within timeout | Increase `start-period` in task definition |
| SignalR disconnections | Missing sticky sessions | Verify ALB target group has sticky sessions enabled |
| 502 Bad Gateway | Container not yet healthy | Wait for startup; check `/ready` endpoint |

---

## Cost Estimation (Monthly, US East)

| Resource | Estimated Cost |
|---|---|
| ECS Fargate (2 tasks, 0.5 vCPU, 1GB) | ~$30 |
| ALB | ~$18 |
| RDS SQL Server Express (db.t3.small) | ~$35 |
| NAT Gateway | ~$35 |
| CloudWatch Logs | ~$2 |
| ECR Storage | ~$1 |
| **Total** | **~$121/month** |

> **Cost Saving Tips**:
> - Use 1 task instead of 2 for non-critical environments (~$15 savings)
> - Use RDS Reserved Instances for 1-year commit (~30% savings)
> - Consider NAT Gateway alternatives (VPC endpoints for ECR) for production

---

## Stack Updates

To update infrastructure after modifying `cloudformation.yml`:

```powershell
aws cloudformation update-stack `
    --stack-name bise-hyderabad-production `
    --template-body file://aws/cloudformation.yml `
    --capabilities CAPABILITY_NAMED_IAM `
    --parameters `
        ParameterKey=DBMasterPassword,UsePreviousValue=true `
        ParameterKey=SeedPassword,UsePreviousValue=true
```

---

## Teardown

> **Warning**: This will delete ALL resources including the database. RDS will create a final snapshot.

```powershell
aws cloudformation delete-stack --stack-name bise-hyderabad-production
aws cloudformation wait stack-delete-complete --stack-name bise-hyderabad-production
```
