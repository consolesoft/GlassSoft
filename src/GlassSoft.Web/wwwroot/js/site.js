// GLASSOFT ERP - Site JavaScript

// ===== Tema Yönetimi (FOUC önlemek için ready() dışında) =====
(function() {
    var savedTheme = localStorage.getItem('gs-theme') || 'dark';
    var body = document.getElementById('mainBody');
    if (body) {
        if (savedTheme === 'light') {
            body.classList.remove('dark-mode');
        } else {
            body.classList.add('dark-mode');
        }
    }
    // Login sayfası için (mainBody olmayabilir)
    if (!body && savedTheme === 'light') {
        document.body.classList.remove('dark-mode');
    }
})();

$(document).ready(function () {

    // Tema toggle
    function applyTheme(theme) {
        var isDark = theme === 'dark';
        var $body = $('body');
        var $navbar = $('#mainNavbar');
        var $icon = $('#themeIcon');

        if (isDark) {
            $body.addClass('dark-mode');
            $navbar.removeClass('navbar-light navbar-white').addClass('navbar-dark');
            $icon.removeClass('fa-moon').addClass('fa-sun');
        } else {
            $body.removeClass('dark-mode');
            $navbar.removeClass('navbar-light navbar-white').addClass('navbar-dark');
            $icon.removeClass('fa-sun').addClass('fa-moon');
        }
        localStorage.setItem('gs-theme', theme);
    }

    // Sayfa yüklendiğinde tema uygula
    var currentTheme = localStorage.getItem('gs-theme') || 'dark';
    applyTheme(currentTheme);

    // Toggle butonu
    $('#themeToggle').on('click', function(e) {
        e.preventDefault();
        var newTheme = localStorage.getItem('gs-theme') === 'dark' ? 'light' : 'dark';
        applyTheme(newTheme);
    });
    // Toastr defaults
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: 3000
    };

    // DataTable defaults (Turkish)
    if ($.fn.DataTable) {
        $.extend(true, $.fn.dataTable.defaults, {
            language: {
                url: "https://cdn.datatables.net/plug-ins/1.13.7/i18n/tr.json"
            },
            responsive: true,
            autoWidth: false,
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>t<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>'
        });
    }

    // Select2 - tüm form select'lerine otomatik uygula
    if ($.fn.select2) {
        $.fn.select2.defaults.set("theme", "default");
        $.fn.select2.defaults.set("language", "tr");
        // Formlar içindeki select elemanlarına Select2 uygula (table içindeki hariç)
        $('select.form-control').not('.no-select2').not('table select').each(function() {
            var $el = $(this);
            // Modal içindeki select'ler için dropdownParent ayarla
            var $modal = $el.closest('.modal');
            var opts = { width: '100%', placeholder: $el.find('option[value=""]').text() || 'Seçiniz...' };
            if ($modal.length) opts.dropdownParent = $modal;
            $el.select2(opts);
        });
    }

    // ===== Yazdır Modal Fonksiyonları =====
    if ($('#printModal').length) {
        var templatesUrl = $('#printModal').data('templates-url');
        var renderUrl = $('#printModal').data('render-url');

        window.openPrintModal = function(outputType, entityId) {
            $('#printOutputType').val(outputType);
            $('#printEntityId').val(entityId);

            if (outputType == 3 || outputType == 4) {
                $('#dateRangeSection').show();
                var today = new Date();
                var monthAgo = new Date();
                monthAgo.setMonth(monthAgo.getMonth() - 1);
                $('#printEndDate').val(today.toISOString().split('T')[0]);
                $('#printStartDate').val(monthAgo.toISOString().split('T')[0]);
            } else {
                $('#dateRangeSection').hide();
            }

            $('#printTemplateSelect').html('<option value="">Yükleniyor...</option>');
            $.get(templatesUrl, { type: outputType }, function(data) {
                var html = '';
                if (data.length === 0) {
                    html = '<option value="">Şablon bulunamadı</option>';
                } else {
                    data.forEach(function(t) {
                        var selected = t.isDefault ? ' selected' : '';
                        html += '<option value="' + t.id + '"' + selected + '>' + t.name + (t.isDefault ? ' (Varsayılan)' : '') + '</option>';
                    });
                }
                $('#printTemplateSelect').html(html);
            });

            $('#printModal').modal('show');
        };

        window.buildPrintUrl = function(autoPrint) {
            var type = $('#printOutputType').val();
            var entityId = $('#printEntityId').val();
            var templateId = $('#printTemplateSelect').val();

            if (!templateId) {
                toastr.warning('Lütfen bir şablon seçin.');
                return null;
            }

            var url = renderUrl + '?type=' + type + '&entityId=' + entityId + '&templateId=' + templateId;

            if (type == 3 || type == 4) {
                var start = $('#printStartDate').val();
                var end = $('#printEndDate').val();
                if (start) url += '&startDate=' + start;
                if (end) url += '&endDate=' + end;
            }

            if (autoPrint) url += '&autoPrint=true';
            return url;
        };

        $('#btnPrintPreview').on('click', function() {
            var url = buildPrintUrl(false);
            if (url) window.open(url, '_blank');
        });

        $('#btnPrintDirect').on('click', function() {
            var url = buildPrintUrl(true);
            if (url) window.open(url, '_blank');
        });
    }

    // SweetAlert2 delete confirmation
    $(document).on("click", ".btn-delete", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var isDark = $('body').hasClass('dark-mode');
        Swal.fire({
            title: "Emin misiniz?",
            text: "Bu kayıt silinecek!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#dc3545",
            cancelButtonColor: "#6c757d",
            confirmButtonText: "Evet, Sil!",
            cancelButtonText: "İptal",
            background: isDark ? "#16213e" : "#fff",
            color: isDark ? "#e0e0e0" : "#333"
        }).then(function (result) {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });
});
