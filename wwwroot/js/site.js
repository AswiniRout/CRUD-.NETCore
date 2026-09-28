// ============================================================
//  Student CRUD - front-end logic (vanilla JavaScript).
//  Talks to the ASP.NET Core Web API at /api/students.
// ============================================================

// Base URL of the API (relative, so it works on any host/port).
const API_URL = "/api/students";

// Grab the elements we interact with once, up front.
const tableBody = document.getElementById("student-table-body");
const formTitle = document.getElementById("form-title");
const idField = document.getElementById("student-id");
const firstNameField = document.getElementById("first-name");
const lastNameField = document.getElementById("last-name");
const emailField = document.getElementById("email");
const ageField = document.getElementById("age");
const messageBox = document.getElementById("message");
const saveButton = document.getElementById("save-button");
const cancelButton = document.getElementById("cancel-button");

// ---------------- Helpers ----------------

// Show a message in red (error) or green (success).
function showMessage(text, isError) {
    messageBox.textContent = text;
    messageBox.className = isError ? "message message-error" : "message message-success";
}

// Clear the form and reset it back to "Add" mode.
function resetForm() {
    idField.value = "";
    firstNameField.value = "";
    lastNameField.value = "";
    emailField.value = "";
    ageField.value = "";
    formTitle.textContent = "Add Student";
    showMessage("", false);
}

// Basic client-side validation before we call the API.
function getValidatedStudent() {
    const firstName = firstNameField.value.trim();
    const lastName = lastNameField.value.trim();
    const email = emailField.value.trim();
    const age = parseInt(ageField.value, 10);

    if (!firstName || !lastName || !email) {
        showMessage("First name, last name and email are required.", true);
        return null;
    }
    if (isNaN(age) || age < 1 || age > 120) {
        showMessage("Age must be between 1 and 120.", true);
        return null;
    }

    return { firstName, lastName, email, age };
}

// ---------------- Read (load all students) ----------------

async function loadStudents() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Failed to load students.");

        const students = await response.json();
        renderTable(students);
    } catch (error) {
        showMessage(error.message, true);
    }
}

// Build the table rows from the list of students.
function renderTable(students) {
    tableBody.innerHTML = "";

    if (students.length === 0) {
        tableBody.innerHTML =
            '<tr><td colspan="5" class="empty-row">No students yet. Add one above.</td></tr>';
        return;
    }

    students.forEach(function (student) {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${student.id}</td>
            <td>${student.fullName}</td>
            <td>${student.email}</td>
            <td>${student.age}</td>
            <td>
                <button class="button button-edit" data-id="${student.id}">Edit</button>
                <button class="button button-delete" data-id="${student.id}">Delete</button>
            </td>`;
        tableBody.appendChild(row);
    });
}

// ---------------- Create / Update (Save) ----------------

async function saveStudent() {
    const student = getValidatedStudent();
    if (!student) return;

    const id = idField.value;
    const isEditing = id !== "";

    // POST to create, PUT to update.
    const url = isEditing ? `${API_URL}/${id}` : API_URL;
    const method = isEditing ? "PUT" : "POST";

    try {
        const response = await fetch(url, {
            method: method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(student)
        });

        if (!response.ok) throw new Error("Save failed. Check your input.");

        showMessage(isEditing ? "Student updated." : "Student added.", false);
        resetForm();
        loadStudents();
    } catch (error) {
        showMessage(error.message, true);
    }
}

// ---------------- Edit (fill the form from a row) ----------------

async function editStudent(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`);
        if (!response.ok) throw new Error("Could not load that student.");

        const student = await response.json();

        // Switch the form into "edit" mode and fill it.
        idField.value = student.id;
        firstNameField.value = student.firstName;
        lastNameField.value = student.lastName;
        emailField.value = student.email;
        ageField.value = student.age;
        formTitle.textContent = "Edit Student";
        showMessage("", false);
    } catch (error) {
        showMessage(error.message, true);
    }
}

// ---------------- Delete ----------------

async function deleteStudent(id) {
    if (!confirm("Delete this student?")) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, { method: "DELETE" });
        if (!response.ok) throw new Error("Delete failed.");

        showMessage("Student deleted.", false);
        loadStudents();
    } catch (error) {
        showMessage(error.message, true);
    }
}

// ---------------- Event wiring ----------------

saveButton.addEventListener("click", saveStudent);
cancelButton.addEventListener("click", resetForm);

// Use event delegation for the Edit/Delete buttons, since rows are dynamic.
tableBody.addEventListener("click", function (event) {
    const id = event.target.getAttribute("data-id");
    if (!id) return;

    if (event.target.classList.contains("button-edit")) {
        editStudent(id);
    } else if (event.target.classList.contains("button-delete")) {
        deleteStudent(id);
    }
});

// Load the list as soon as the page opens.
loadStudents();
