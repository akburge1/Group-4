//When the anonymous checkbox is unchecked, display the name field
function toggleNameField() {
    const isAnonymous = document.getElementById("isAnonymous")?.checked;
    const nameRow = document.querySelector(".name-row");
    if (nameRow) {
        nameRow.style.display = isAnonymous ? "none" : "block";
    }
}


function getWeeklyPrompt() {
    return "Placeholder prompt: your real weekly prompt will appear here once connected.";
}

//Show the weekly prompt from the database
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
                fetchFeedback(currentPromptId);
            } else {
                promptElement.textContent = "Unable to load prompt.";
            }
        })
        .catch(() => {
            promptElement.textContent = "Error loading prompt.";
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