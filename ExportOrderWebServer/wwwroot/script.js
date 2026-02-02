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

async function jsOpenPdfInNewTab(byteBase64) {
    try {
        // Декодируем base64
        const binaryString = window.atob(byteBase64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // Создаем Blob объект
        const blob = new Blob([bytes], { type: "application/pdf" });

        // Создаем URL для Blob
        const url = URL.createObjectURL(blob);

        // Открываем в новой вкладке
        const newWindow = window.open(url, '_blank');

        // Если браузер заблокировал всплывающее окно, предоставляем ссылку для клика
        if (!newWindow) {
            // Fallback: создаем ссылку для клика
            const link = document.createElement('a');
            link.href = url;
            link.target = '_blank';            
            link.textContent = `Открыть файл`;  //`Открыть ${filename}`
            link.click();
        }

        // Очистка через минуту
        setTimeout(() => URL.revokeObjectURL(url), 60000);

        //------------------------------------------------------------------------------
        //// Для небольших файлов можно использовать data URL напрямую
        //const dataUrl = `data:application/pdf;base64,${byteBase64}`;
        //const newWindow = window.open(dataUrl, '_blank');
        //// Освобождаем URL через некоторое время
        //setTimeout(() => { URL.revokeObjectURL(url); }, 60000);
        //------------------------------------------------------------------------------
    }
    catch (error) {
        console.error('Error loading PDF:', error);
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
            // РџС‹С‚Р°РµРјСЃСЏ Р·Р°РєСЂС‹С‚СЊ РІРєР»Р°РґРєСѓ
            window.close();

            // Р•СЃР»Рё window.close() РЅРµ СЃСЂР°Р±РѕС‚Р°Р», РЅРѕ Рё РЅРµ РІС‹Р±СЂРѕСЃРёР» РѕС€РёР±РєСѓ
            setTimeout(() => {
                if (!window.closed) {
                    alert("Pls close Window manually, since it was open that way (Ctrl + W / Cmd + W)");
                }
            }, 100);
        } catch (e) {
            // Р•СЃР»Рё Р±СЂР°СѓР·РµСЂ СЏРІРЅРѕ Р·Р°РїСЂРµС‚РёР» Р·Р°РєСЂС‹С‚РёРµ
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

            // Р’С‹С‡РёСЃР»СЏРµРј РґРѕСЃС‚СѓРїРЅСѓСЋ РІС‹СЃРѕС‚Сѓ РѕС‚ С‚РµРєСѓС‰РµР№ РїРѕР·РёС†РёРё С‚Р°Р±Р»РёС†С‹ РґРѕ РЅРёР·Р° РѕРєРЅР°
            const availableHeight = windowHeight - containerRect.top - padding;
            const finalHeight = Math.max(500, availableHeight);

            tableContainer.style.maxHeight = `${finalHeight}px`;
            console.log('вњ“ Table height calculated:', finalHeight, 'px');
            return true;
        } else if (attempts < maxAttempts) {
            attempts++;
            console.log(`Attempt ${attempts} to find elements...`);
            setTimeout(tryCalculate, 200);
            return false;
        } else {
            console.log('вњ— Failed to calculate table height after', maxAttempts, 'attempts');
            return false;
        }
    }

    tryCalculate();
}

// Р—Р°РїСѓСЃРєР°РµРј РєРѕРіРґР° DOM РіРѕС‚РѕРІ
document.addEventListener('DOMContentLoaded', calculateTableHeight);

// РўР°РєР¶Рµ РЅР° resize СЃ debounce
let resizeTimeout;
window.addEventListener('resize', function () {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(calculateTableHeight, 100);
});

// Р РЅР° РїРѕР»РЅСѓСЋ Р·Р°РіСЂСѓР·РєСѓ СЃС‚СЂР°РЅРёС†С‹
window.addEventListener('load', calculateTableHeight);