document.addEventListener("DOMContentLoaded", function () {
    // Alerta de carga de archivo Excel
    var archivoInput = document.getElementById("archivoExcel");
    var form = document.getElementById("uploadForm");

    form.addEventListener("submit", function (e) {
        // Validar si el archivo es un archivo Excel (.xlsx)
        var archivo = archivoInput.files[0];
        if (!archivo) {
            e.preventDefault();
            Swal.fire({
                icon: 'warning',
                title: 'Error',
                text: 'Debe seleccionar un archivo Excel para cargar.',
                confirmButtonText: 'Aceptar'
            });
        } else if (archivo.name.split('.').pop().toLowerCase() !== 'xlsx') {
            e.preventDefault();
            Swal.fire({
                icon: 'error',
                title: 'Formato inválido',
                text: 'El archivo seleccionado no es un archivo Excel válido. Por favor, seleccione un archivo con la extensión .xlsx.',
                confirmButtonText: 'Aceptar'
            });
        } else {
            // Muestra la alerta de carga
            Swal.fire({
                icon: 'info',
                title: 'Cargando...',
                text: 'El archivo está siendo cargado. Por favor, espere.',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            var percent = 0;
            var interval = setInterval(function () {
                percent += 10;
                Swal.getContent().querySelector('p').textContent = `Cargando: ${percent}%`;
                if (percent >= 100) {
                    clearInterval(interval);
                }
            }, 1000);
        }
    });

    // Verificar si hay datos para exportar
    var exportExcelButton = document.getElementById("exportExcel");
    var exportPDFButton = document.getElementById("exportPDF");
    var tableRows = document.querySelectorAll("table tbody tr");

    if (tableRows.length === 0) {
        exportExcelButton.addEventListener("click", function (e) {
            e.preventDefault();
            Swal.fire({
                icon: 'warning',
                title: 'No hay datos para exportar',
                text: 'No se han cargado obreros para exportar.',
                confirmButtonText: 'Aceptar'
            });
        });

        exportPDFButton.addEventListener("click", function (e) {
            e.preventDefault();
            Swal.fire({
                icon: 'warning',
                title: 'No hay datos para exportar',
                text: 'No se han cargado obreros para exportar.',
                confirmButtonText: 'Aceptar'
            });
        });
    }
});
