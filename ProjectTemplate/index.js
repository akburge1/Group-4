function displayPrompt() {
    const promptElement = document.getElementById("weekly-prompt");
    if (promptElement) {
        promptElement.textContent = getWeeklyPrompt();
    }
    //fetch('/api/prompt')

}

function handleFormSubmission() {
    const form = document.querySelector(".feedback-form");

    form.addEventListener("submit", function (e) {
        e.preventDefault(); // Prevent actual HTTP POST
        alert("Feedback submitted!"); //testing
        form.reset();
    });
}

document.addEventListener("DOMContentLoaded", () => {
    displayPrompt();
    handleFormSubmission();
});
