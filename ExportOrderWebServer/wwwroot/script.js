function jsSaveAsFile(filename, byteBase64) {
  var link = document.createElement('a');
  link.download = filename;
  link.href = "data:application/octet-stream;base64," + byteBase64;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

window.focusLastRow = function () {
    const rows = document.querySelectorAll('.mud-table-body tr');
    if (rows.length > 0) {
        const lastRow = rows[rows.length - 1];
        lastRow.focus();
        lastRow.scrollIntoView({ behavior: 'smooth', block: 'end' });
    }
}

function closeWindow() {
    if (confirm("Do you want to close Window?")) {
        try {
            // Пытаемся закрыть вкладку
            window.close();

            // Если window.close() не сработал, но и не выбросил ошибку
            setTimeout(() => {
                if (!window.closed) {
                    alert("Pls close Window manually, since it was open that way (Ctrl + W / Cmd + W)");
                }
            }, 100);
        } catch (e) {
            // Если браузер явно запретил закрытие
            alert("Pls close Window manually, since it was open that way (Ctrl + W / Cmd + W)");
        }
    }
}