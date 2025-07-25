document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('login-form');
    const errorMessage = document.getElementById('error-message');

    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;

        errorMessage.style.display = 'none';
        errorMessage.textContent = '';

        try {
            const response = await fetch('ProjectServices.asmx/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const result = await response.json();
            const role = result.d;  // ASMX wraps response in .d

            if (role === 'invalid') {
                errorMessage.textContent = 'Invalid username or password.';
                errorMessage.style.display = 'block';
            } else if (role === 'employee') {
                window.location.href = 'index.html';
            } else if (role === 'admin') {
                window.location.href = 'admin.html';
            }
        } catch (error) {
            errorMessage.textContent = 'An error occurred during login. Please try again.';
            errorMessage.style.display = 'block';
        }
    });
});