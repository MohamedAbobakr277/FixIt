// ── Dark Mode Toggle ──
const themeToggle = document.getElementById('themeToggle');
let savedTheme = localStorage.getItem('theme');

// Force light theme as default if nothing is saved
if (!savedTheme) {
    savedTheme = 'light';
    localStorage.setItem('theme', 'light');
}

document.documentElement.setAttribute('data-theme', savedTheme);
if (themeToggle) themeToggle.textContent = savedTheme === 'dark' ? '☀️' : '🌙';

if (themeToggle) {
    themeToggle.addEventListener('click', () => {
        const current = document.documentElement.getAttribute('data-theme');
        const next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        localStorage.setItem('theme', next);
        themeToggle.textContent = next === 'dark' ? '☀️' : '🌙';
    });
}

// ── Navbar scroll effect & Scroll Progress ──
window.addEventListener('scroll', () => {
    const nav = document.getElementById('mainNavbar');
    if (nav) nav.classList.toggle('scrolled', window.scrollY > 50);

    const scrollProgress = document.getElementById('scrollProgress');
    if (scrollProgress) {
        const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
        const scrollHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        const progress = (scrollTop / scrollHeight) * 100;
        scrollProgress.style.width = progress + '%';
    }

    const scrollToTopBtn = document.getElementById('scrollToTopBtn');
    if (scrollToTopBtn) {
        if (window.scrollY > 300) {
            scrollToTopBtn.classList.add('show');
        } else {
            scrollToTopBtn.classList.remove('show');
        }
    }
});

// ── Mobile toggle ──
const navToggle = document.getElementById('navToggle');
const navLinks = document.getElementById('navLinks');
const navActions = document.getElementById('navActions');
if (navToggle) {
    navToggle.addEventListener('click', () => {
        navLinks?.classList.toggle('active');
        navActions?.classList.toggle('active');
    });
}

// ── Smooth scroll ──
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            e.preventDefault();
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            navLinks?.classList.remove('active');
            navActions?.classList.remove('active');
        }
    });
});

// ── Scroll to Top Button ──
const scrollBtn = document.getElementById('scrollToTopBtn');
if (scrollBtn) {
    scrollBtn.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

// ── Scroll reveal & Number Counters ──
const animateCounter = (el) => {
    if (el.classList.contains('is-counting')) return;
    el.classList.add('is-counting');

    const targetAttr = el.getAttribute('data-target');
    const target = parseFloat(targetAttr);
    const isFloat = targetAttr.includes('.');
    const duration = 2500; // 2.5 seconds
    
    // Wait for the page reveal animations to finish before counting
    setTimeout(() => {
        let startTime = null;

        const updateCounter = (currentTime) => {
            if (!startTime) startTime = currentTime;
            const elapsedTime = currentTime - startTime;
            
            if (elapsedTime < duration) {
                // easeOutQuart formula for smooth deceleration
                const progress = 1 - Math.pow(1 - (elapsedTime / duration), 4);
                const current = target * progress;
                
                el.textContent = isFloat ? current.toFixed(1) : Math.floor(current);
                requestAnimationFrame(updateCounter);
            } else {
                el.textContent = isFloat ? target.toFixed(1) : target;
            }
        };
        requestAnimationFrame(updateCounter);
    }, 400);
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0) scale(1)';
            entry.target.style.filter = 'blur(0)';
            
            // Trigger counters if inside the target
            const counters = entry.target.querySelectorAll('.counter');
            if(counters.length) counters.forEach(animateCounter);
            if(entry.target.classList.contains('counter')) animateCounter(entry.target);

            observer.unobserve(entry.target);
        }
    });
}, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.feature-card, .step-card, .category-card, .hero-stats').forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px) scale(0.95)';
        el.style.filter = 'blur(4px)';
        const delay = (index % 3) * 0.1;
        el.style.transition = `opacity 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) ${delay}s, transform 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) ${delay}s, filter 0.6s ease ${delay}s`;
        observer.observe(el);
    });

    // ── 3D Hero Parallax ──
    const heroCard = document.querySelector('.hero-card');
    const heroVisual = document.querySelector('.hero-visual');
    
    if (heroCard && heroVisual) {
        heroVisual.addEventListener('mousemove', (e) => {
            heroCard.style.animation = 'none';
            heroCard.style.transition = 'transform 0.1s ease-out';
            
            const rect = heroVisual.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            
            const xAxis = (centerX - x) / 15;
            const yAxis = (centerY - y) / 15;
            
            heroCard.style.transform = `rotateY(${xAxis}deg) rotateX(${yAxis}deg) scale(1.02)`;
        });

        heroVisual.addEventListener('mouseleave', () => {
            heroCard.style.transition = 'transform 0.5s ease-in-out';
            heroCard.style.transform = 'rotateY(0deg) rotateX(0deg) scale(1)';
            setTimeout(() => {
                heroCard.style.animation = 'floatCard 6s ease-in-out infinite';
            }, 500);
        });
    }
});
