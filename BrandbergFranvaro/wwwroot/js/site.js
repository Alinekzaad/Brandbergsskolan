// Brandbergsskolan - Frånvarohantering JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Initiera datumvalidering
    initDateValidation();
    
    // Initiera formulärvalidering
    initFormValidation();
    
    // Initiera omfattningsväljare
    initDayPartSelector();
    
    // Initiera kommentarsvalidering för "Annat"
    initCommentValidation();
    
    // Initiera bekräftelsedialoger
    initConfirmDialogs();
});

// Datumvalidering
function initDateValidation() {
    const startDateInput = document.getElementById('StartDate');
    const endDateInput = document.getElementById('EndDate');
    
    if (startDateInput && endDateInput) {
        // Sätt min-datum till idag
        const today = new Date().toISOString().split('T')[0];
        startDateInput.setAttribute('min', today);
        
        // Sätt max-datum till 365 dagar framåt
        const maxDate = new Date();
        maxDate.setDate(maxDate.getDate() + 365);
        const maxDateStr = maxDate.toISOString().split('T')[0];
        startDateInput.setAttribute('max', maxDateStr);
        endDateInput.setAttribute('max', maxDateStr);
        
        // Uppdatera slutdatums min-värde när startdatum ändras
        startDateInput.addEventListener('change', function () {
            endDateInput.setAttribute('min', this.value);
            if (endDateInput.value && endDateInput.value < this.value) {
                endDateInput.value = this.value;
            }
        });
        
        // Validera att slutdatum inte är före startdatum
        endDateInput.addEventListener('change', function () {
            if (startDateInput.value && this.value < startDateInput.value) {
                this.setCustomValidity('Slutdatum kan inte vara före startdatum.');
            } else {
                this.setCustomValidity('');
            }
        });
    }
}

// Formulärvalidering (Bootstrap 5)
function initFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    
    forms.forEach(function (form) {
        form.addEventListener('submit', function (event) {
            // Extra validering för kommentar vid "Annat"
            const typeSelect = form.querySelector('#Type');
            const commentInput = form.querySelector('#Comment');
            
            if (typeSelect && commentInput) {
                if (typeSelect.value === '5' && !commentInput.value.trim()) {
                    commentInput.setCustomValidity('Kommentar krävs när typ är "Annat".');
                } else {
                    commentInput.setCustomValidity('');
                }
            }
            
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            
            form.classList.add('was-validated');
        });
    });
}

// Visa/dölj halvdagsalternativ baserat på omfattning
function initDayPartSelector() {
    const dayPartSelect = document.getElementById('DayPart');
    const halfDayOptions = document.getElementById('halfDayOptions');
    
    if (dayPartSelect) {
        dayPartSelect.addEventListener('change', function () {
            // Heldag = 1, Förmiddag = 2, Eftermiddag = 3
            // Vi visar halvdagsinfo om inte heldag
            updateHalfDayDisplay(this.value);
        });
    }
}

function updateHalfDayDisplay(value) {
    const halfDayInfo = document.getElementById('halfDayInfo');
    if (halfDayInfo) {
        if (value === '2' || value === '3') {
            halfDayInfo.style.display = 'block';
        } else {
            halfDayInfo.style.display = 'none';
        }
    }
}

// Kommentarsvalidering för typ "Annat"
function initCommentValidation() {
    const typeSelect = document.getElementById('Type');
    const commentInput = document.getElementById('Comment');
    const commentHelp = document.getElementById('commentHelp');
    
    if (typeSelect && commentInput) {
        typeSelect.addEventListener('change', function () {
            updateCommentRequirement(this.value, commentInput, commentHelp);
        });
        
        // Initial check
        if (typeSelect.value) {
            updateCommentRequirement(typeSelect.value, commentInput, commentHelp);
        }
    }
}

function updateCommentRequirement(typeValue, commentInput, commentHelp) {
    const isAnnat = typeValue === '5'; // Annat = 5
    
    if (isAnnat) {
        commentInput.setAttribute('required', 'required');
        if (commentHelp) {
            commentHelp.innerHTML = '<i class="bi bi-exclamation-circle"></i> Kommentar är obligatorisk för typ "Annat".';
            commentHelp.classList.add('text-warning');
        }
    } else {
        commentInput.removeAttribute('required');
        if (commentHelp) {
            commentHelp.textContent = 'Valfritt. Max 1000 tecken.';
            commentHelp.classList.remove('text-warning');
        }
    }
}

// Bekräftelsedialoger för borttagning
function initConfirmDialogs() {
    const deleteButtons = document.querySelectorAll('[data-confirm]');
    
    deleteButtons.forEach(function (button) {
        button.addEventListener('click', function (event) {
            const message = this.getAttribute('data-confirm') || 'Är du säker?';
            if (!confirm(message)) {
                event.preventDefault();
            }
        });
    });
}

// Filuppladdningsvalidering
function validateFileUpload(input) {
    const maxSize = 5 * 1024 * 1024; // 5 MB
    const allowedTypes = ['.pdf', '.jpg', '.jpeg', '.png'];
    const feedback = document.getElementById('fileFeedback');
    
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const extension = '.' + file.name.split('.').pop().toLowerCase();
        
        // Kontrollera filtyp
        if (!allowedTypes.includes(extension)) {
            input.value = '';
            if (feedback) {
                feedback.textContent = 'Ogiltig filtyp. Tillåtna typer: PDF, JPG, PNG';
                feedback.className = 'invalid-feedback d-block';
            }
            return false;
        }
        
        // Kontrollera storlek
        if (file.size > maxSize) {
            input.value = '';
            if (feedback) {
                feedback.textContent = 'Filen är för stor. Maximal storlek är 5 MB.';
                feedback.className = 'invalid-feedback d-block';
            }
            return false;
        }
        
        if (feedback) {
            feedback.textContent = 'Fil vald: ' + file.name + ' (' + formatFileSize(file.size) + ')';
            feedback.className = 'valid-feedback d-block';
        }
    }
    
    return true;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Exportera CSV
function exportTableToCSV(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) return;
    
    let csv = [];
    const rows = table.querySelectorAll('tr');
    
    rows.forEach(function (row) {
        const cols = row.querySelectorAll('td, th');
        const rowData = [];
        
        cols.forEach(function (col) {
            // Ta bort knappar och ikoner från export
            let text = col.innerText.replace(/[\n\r]+/g, ' ').trim();
            // Escape dubbla citattecken
            text = text.replace(/"/g, '""');
            rowData.push('"' + text + '"');
        });
        
        csv.push(rowData.join(';'));
    });
    
    // Skapa och ladda ner fil
    const csvContent = '\uFEFF' + csv.join('\n'); // BOM för Excel-kompatibilitet
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    
    link.href = URL.createObjectURL(blob);
    link.download = filename || 'export.csv';
    link.click();
}

