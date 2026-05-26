// BankApp JavaScript

// Add active class to current nav link
document.addEventListener('DOMContentLoaded', function() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.navbar-nav a');
    
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
});

// Toggle password visibility
function togglePassword(inputId) {
    const input = document.getElementById(inputId);
    const icon = document.getElementById(inputId + '-icon');
    
    if (input.type === 'password') {
        input.type = 'text';
        icon.textContent = '🙈';
    } else {
        input.type = 'password';
        icon.textContent = '👁️';
    }
}

// Theme Toggle Functions
function toggleTheme() {
    const html = document.documentElement;
    const currentTheme = html.getAttribute('data-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    
    html.setAttribute('data-theme', newTheme);
    localStorage.setItem('theme', newTheme);
    
    updateThemeButton(newTheme);
}

function updateThemeButton(theme) {
    const btn = document.getElementById('theme-toggle-btn');
    const icon = document.getElementById('theme-icon');
    const text = document.getElementById('theme-text');
    
    if (btn && icon && text) {
        if (theme === 'dark') {
            icon.textContent = '☀️';
            text.textContent = 'Switch to Light Mode';
        } else {
            icon.textContent = '🌙';
            text.textContent = 'Switch to Dark Mode';
        }
    }
}

// Initialize theme button on settings page
document.addEventListener('DOMContentLoaded', function() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    updateThemeButton(savedTheme);
});
