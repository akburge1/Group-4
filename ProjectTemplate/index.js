function displayPrompt() {
    const promptElement = document.getElementById("weekly-prompt");

    fetch("/ProjectServices.asmx/GetCurrentPrompt", {
        method: "POST",
        headers: { "Content-Type": "application/json" }
    })
        .then(res => res.json())
        .then(data => {
            const prompt = data?.d;
            if (prompt && prompt.text) {
                promptElement.textContent = prompt.text;
                currentPromptId = prompt.id; // store for use in submit
            } else {
                promptElement.textContent = "Unable to load prompt.";
            }
        })
        .catch(() => {
            promptElement.textContent = "Error loading prompt.";
        });
}


function handleFormSubmission() {
    const form = document.querySelector(".feedback-form");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault(); // Prevent actual HTTP POST
        alert("Feedback submitted!"); //testing
        form.reset();
    });
}



document.addEventListener("DOMContentLoaded", () => {
    displayPrompt();
    handleFormSubmission();

    const anonCheckbox = document.getElementById("isAnonymous");
    if (anonCheckbox) {
        anonCheckbox.addEventListener("change", toggleNameField);
        toggleNameField(); // set initial state
    }
});