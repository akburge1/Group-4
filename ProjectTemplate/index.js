// Placeholder for weekly prompt service
function getWeeklyPrompt() {
    return "Placeholder prompt: your real weekly prompt will appear here once connected.";
}

function displayPrompt() {
    const promptElement = document.getElementById("weekly-prompt");
    if (promptElement) {
        promptElement.textContent = getWeeklyPrompt();
    }
    // Future: fetch('/api/prompt').then(...)
}

function handleFormSubmission() {
    const form = document.querySelector(".feedback-form");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        const feedback = (document.getElementById("feedback")?.value || "").trim();
        const isAnonymous = document.getElementById("isAnonymous")?.checked ?? true;

        if (!feedback) {
            alert("Please enter feedback before submitting.");
            return;
        }

        // Placeholder for eventual POST
        // fetch('/api/submit-feedback', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ feedback, isAnonymous }) })

        alert(`Feedback submitted!\n\nAnonymous: ${isAnonymous ? "Yes" : "No"}`);
        form.reset(); // resets to initial defaults (anonymous remains checked)
        document.getElementById("isAnonymous").checked = true; // ensure default after reset
    });
}

document.addEventListener("DOMContentLoaded", () => {
    displayPrompt();
    handleFormSubmission();
});
