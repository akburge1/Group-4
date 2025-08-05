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
    const errorDiv = document.getElementById("feedback-error");
    if (!form) return;

    form.addEventListener("submit", function (e) {
        e.preventDefault();

        errorDiv.style.display = "none";
        errorDiv.textContent = "";
        const feedback = (document.getElementById("feedback")?.value || "").trim();
        const isAnonymous = document.getElementById("isAnonymous")?.checked ?? true;

        if (!feedback) {
            errorDiv.textContent = "Feedback message cannot be empty.";
            errorDiv.style.display = "block";
            return;
        }

        fetch("/ProjectServices.asmx/SubmitFeedback", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                message: feedback,
                isAnonymous: isAnonymous
            })
        })
            .then(res => res.json())
            .then(data => {
                const result = data?.d;
                switch (result) {
                    case "success":
                        alert(isAnonymous
                            ? "Thank you! Your anonymous feedback has been submitted."
                            : "Thank you! Your feedback has been submitted.");
                        form.reset();
                        toggleNameField();
                        break;
                    case "already_submitted":
                        alert("You've already submitted feedback for this week's prompt.");
                        break;
                    case "not_authenticated":
                        alert("You must be logged in to submit feedback.");
                        break;
                    case "no_current_prompt":
                        alert("No current prompt is available.");
                        break;
                    default:
                        if (result?.startsWith("error:")) {
                            alert("Server error:\n" + result);
                        } else {
                            alert("Unexpected response: " + result);
                        }
                        break;
                }
            })
            .catch(err => {
                console.error("Submit error:", err);
                alert("Submission failed due to a network or server issue.");
            });
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