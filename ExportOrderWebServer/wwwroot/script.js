function jsSaveAsFile(filename, byteBase64) {
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
function downloadFileBase64(byteBase64, fileName) {
    try {
        const link = document.createElement('a');
        link.href = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + byteBase64;
        link.download = fileName;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return true;
    } catch (error) {
        console.error('Download error:', error);
        return false;
    }
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

function calculateTableHeight(maxAttempts = 10) {
    let attempts = 0;

    function tryCalculate() {
        const tableContainer = document.querySelector('.Table .mud-table-container');
        const header = document.querySelector('.ToolBar');

        if (tableContainer && header && header.offsetHeight > 0) {
            const headerHeight = header.offsetHeight;
            const containerRect = tableContainer.getBoundingClientRect();
            const windowHeight = window.innerHeight;
            const padding = 30;

            // Вычисляем доступную высоту от текущей позиции таблицы до низа окна
            const availableHeight = windowHeight - containerRect.top - padding;
            const finalHeight = Math.max(500, availableHeight);

            tableContainer.style.maxHeight = `${finalHeight}px`;
            console.log('✓ Table height calculated:', finalHeight, 'px');
            return true;
        } else if (attempts < maxAttempts) {
            attempts++;
            console.log(`Attempt ${attempts} to find elements...`);
            setTimeout(tryCalculate, 200);
            return false;
        } else {
            console.log('✗ Failed to calculate table height after', maxAttempts, 'attempts');
            return false;
        }
    }

    tryCalculate();
}

// Запускаем когда DOM готов
document.addEventListener('DOMContentLoaded', calculateTableHeight);

// Также на resize с debounce
let resizeTimeout;
window.addEventListener('resize', function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(calculateTableHeight, 100);
});

// И на полную загрузку страницы
window.addEventListener('load', calculateTableHeight);