// BISE Sukkur Web UI Utilities

window.biseApp = {
    print: function () {
        window.print();
    },
    toggleTheme: function () {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('bise_theme', newTheme);
        return newTheme;
    },
    initTheme: function () {
        const savedTheme = localStorage.getItem('bise_theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
    },
    copyToClipboard: async function (text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            console.error('Failed to copy: ', err);
            return false;
        }
    },
    downloadFile: function (filename, contentType, content) {
        const blob = new Blob([content], { type: contentType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },
    toggleSidebar: function () {
        document.body.classList.toggle('sidebar-open');
    },
    closeSidebar: function () {
        document.body.classList.remove('sidebar-open');
    },
    submitLogout: function () {
        const form = document.getElementById('logoutForm');
        if (form) form.submit();
    },
    setInputValue: function (id, value) {
        const el = document.getElementById(id);
        if (el) {
            el.value = value ?? '';
            el.dispatchEvent(new Event('input', { bubbles: true }));
        }
    },
    togglePasswordType: function (id) {
        const el = document.getElementById(id);
        if (!el) return 'password';
        el.type = el.type === 'password' ? 'text' : 'password';
        return el.type;
    }
};

document.addEventListener('DOMContentLoaded', () => {
    window.biseApp.initTheme();

    document.addEventListener('click', (e) => {
        if (window.innerWidth <= 768) {
            if (e.target.closest('.nav-item a') || e.target.closest('.sidebar-overlay')) {
                window.biseApp.closeSidebar();
            }
        }
    });
});
