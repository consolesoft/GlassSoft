/**
 * GLASSOFT - Görsel Şablon Editörü (Drag & Drop)
 * Blok tabanlı şablon oluşturucu
 */
var TemplateEditor = (function () {
    var blocks = [];
    var blockIdCounter = 0;
    var activeBlockId = null;
    var sortableInstance = null;

    // Blok tür tanımları
    var blockTypes = {
        header: {
            icon: 'fas fa-heading',
            label: 'Başlık',
            color: '#007bff',
            defaults: { title: 'GLASSOFT', subtitle: '{{SiparisNo}}', align: 'center', bgColor: '#2c3e50', textColor: '#ffffff' }
        },
        docinfo: {
            icon: 'fas fa-info-circle',
            label: 'Belge Bilgisi',
            color: '#17a2b8',
            defaults: { layout: '2col' }
        },
        text: {
            icon: 'fas fa-paragraph',
            label: 'Metin',
            color: '#28a745',
            defaults: { content: 'Metin içeriği...', align: 'left', bold: false, fontSize: '13' }
        },
        table: {
            icon: 'fas fa-table',
            label: 'Kalemler Tablosu',
            color: '#fd7e14',
            defaults: { headerBg: '#2c3e50', headerColor: '#ffffff' }
        },
        totals: {
            icon: 'fas fa-calculator',
            label: 'Toplamlar',
            color: '#e83e8c',
            defaults: { align: 'right' }
        },
        signatures: {
            icon: 'fas fa-signature',
            label: 'İmza Alanı',
            color: '#6f42c1',
            defaults: { cols: '2', labels: 'Teslim Eden,Teslim Alan' }
        },
        divider: {
            icon: 'fas fa-minus',
            label: 'Ayırıcı',
            color: '#6c757d',
            defaults: { style: 'solid', color: '#dee2e6' }
        },
        customerinfo: {
            icon: 'fas fa-user',
            label: 'Müşteri Bilgisi',
            color: '#20c997',
            defaults: { showAddress: true, showPhone: true, showEmail: true, showTax: true }
        }
    };

    // Placeholder'a göre alan açıklamaları
    var fieldDescriptions = {
        'SiparisNo': 'Sipariş No', 'SiparisTarihi': 'Sipariş Tarihi', 'TeslimTarihi': 'Teslim Tarihi',
        'TeslimatTarihi': 'Teslimat Tarihi', 'Durum': 'Durum', 'MusteriAdi': 'Müşteri Adı',
        'MusteriKodu': 'Müşteri Kodu', 'MusteriAdres': 'Adres', 'MusteriSehir': 'Şehir',
        'MusteriTelefon': 'Telefon', 'MusteriEmail': 'E-posta', 'MusteriVergiNo': 'Vergi No',
        'MusteriVergiDairesi': 'Vergi Dairesi', 'ParaBirimi': 'Para Birimi', 'ToplamTutar': 'Toplam Tutar',
        'Notlar': 'Notlar', 'ToplamAdet': 'Toplam Adet', 'ToplamAlan': 'Toplam Alan',
        'Tarih': 'Bugünün Tarihi', 'Saat': 'Saat', 'IsEmriNo': 'İş Emri No',
        'IsEmriTarihi': 'İş Emri Tarihi', 'SiparisNolari': 'Sipariş Noları',
        'ToplamBorc': 'Toplam Borç', 'ToplamAlacak': 'Toplam Alacak', 'Bakiye': 'Bakiye',
        'BakiyeYon': 'B/A', 'DevredenBakiye': 'Devreden Bakiye', 'DevredenYon': 'Devreden B/A',
        'BaslangicTarihi': 'Başlangıç', 'BitisTarihi': 'Bitiş', 'EkstreTarihi': 'Ekstre Tarihi'
    };

    function generateId() {
        return 'block_' + (++blockIdCounter);
    }

    function addBlock(type, props, insertAfterId) {
        var bt = blockTypes[type];
        if (!bt) return;
        var id = generateId();
        var block = { id: id, type: type, props: $.extend({}, bt.defaults, props || {}) };

        if (insertAfterId) {
            var idx = blocks.findIndex(function (b) { return b.id === insertAfterId; });
            blocks.splice(idx + 1, 0, block);
        } else {
            blocks.push(block);
        }
        renderCanvas();
        selectBlock(id);
        syncToTextarea();
        return id;
    }

    function removeBlock(id) {
        blocks = blocks.filter(function (b) { return b.id !== id; });
        if (activeBlockId === id) {
            activeBlockId = null;
            renderProperties();
        }
        renderCanvas();
        syncToTextarea();
    }

    function selectBlock(id) {
        activeBlockId = id;
        $('#visualCanvas .ve-block').removeClass('active');
        $('#visualCanvas .ve-block[data-id="' + id + '"]').addClass('active');
        renderProperties();
    }

    function getBlock(id) {
        return blocks.find(function (b) { return b.id === id; });
    }

    function updateBlockProp(id, key, value) {
        var block = getBlock(id);
        if (block) {
            block.props[key] = value;
            renderCanvas();
            syncToTextarea();
            // Keep selection
            $('#visualCanvas .ve-block[data-id="' + id + '"]').addClass('active');
        }
    }

    // Canvas'taki blokları render et
    function renderCanvas() {
        var $canvas = $('#visualCanvas');
        $canvas.empty();

        if (blocks.length === 0) {
            $canvas.html(
                '<div class="ve-empty text-center text-muted py-5">' +
                '<i class="fas fa-arrows-alt fa-3x mb-3" style="opacity:0.3"></i>' +
                '<p>Soldaki bileşenleri buraya sürükleyin<br>veya tıklayarak ekleyin</p></div>'
            );
            return;
        }

        blocks.forEach(function (block) {
            var bt = blockTypes[block.type];
            var preview = renderBlockPreview(block);
            var html =
                '<div class="ve-block" data-id="' + block.id + '" data-type="' + block.type + '">' +
                '<div class="ve-block-header">' +
                '<span class="ve-block-grip"><i class="fas fa-grip-vertical"></i></span>' +
                '<span class="ve-block-type"><i class="' + bt.icon + ' mr-1"></i>' + bt.label + '</span>' +
                '<span class="ve-block-actions">' +
                '<button type="button" class="btn btn-xs btn-outline-info ve-btn-edit" title="Düzenle"><i class="fas fa-cog"></i></button>' +
                '<button type="button" class="btn btn-xs btn-outline-secondary ve-btn-clone" title="Kopyala"><i class="fas fa-copy"></i></button>' +
                '<button type="button" class="btn btn-xs btn-outline-danger ve-btn-remove" title="Sil"><i class="fas fa-trash"></i></button>' +
                '</span></div>' +
                '<div class="ve-block-preview">' + preview + '</div>' +
                '</div>';
            $canvas.append(html);
        });

        // SortableJS ile sıralama
        if (sortableInstance) sortableInstance.destroy();
        sortableInstance = new Sortable($canvas[0], {
            handle: '.ve-block-grip',
            animation: 200,
            ghostClass: 've-ghost',
            onEnd: function () {
                // DOM sırasına göre blocks dizisini güncelle
                var newOrder = [];
                $canvas.find('.ve-block').each(function () {
                    var id = $(this).data('id');
                    var block = getBlock(id);
                    if (block) newOrder.push(block);
                });
                blocks = newOrder;
                syncToTextarea();
            }
        });
    }

    // Blok önizleme HTML'i
    function renderBlockPreview(block) {
        var p = block.props;
        switch (block.type) {
            case 'header':
                return '<div style="text-align:' + (p.align || 'center') + ';background:' + (p.bgColor || '#2c3e50') +
                    ';color:' + (p.textColor || '#fff') + ';padding:12px 16px;border-radius:4px;">' +
                    '<div style="font-size:18px;font-weight:bold;">' + escHtml(p.title || '') + '</div>' +
                    '<div style="font-size:13px;opacity:0.9;">' + escHtml(p.subtitle || '') + '</div></div>';

            case 'docinfo':
                return '<div style="display:flex;gap:20px;padding:8px;font-size:12px;background:#f8f9fa;border-radius:4px;">' +
                    '<div><strong>Belge No:</strong> {{SiparisNo}}</div>' +
                    '<div><strong>Tarih:</strong> {{SiparisTarihi}}</div>' +
                    '<div><strong>Durum:</strong> {{Durum}}</div>' +
                    '<div><strong>Para Birimi:</strong> {{ParaBirimi}}</div></div>';

            case 'customerinfo':
                var fields = ['<strong>{{MusteriAdi}}</strong>'];
                if (p.showAddress) fields.push('{{MusteriAdres}} / {{MusteriSehir}}');
                if (p.showPhone) fields.push('Tel: {{MusteriTelefon}}');
                if (p.showEmail) fields.push('E-posta: {{MusteriEmail}}');
                if (p.showTax) fields.push('VKN: {{MusteriVergiNo}} - {{MusteriVergiDairesi}}');
                return '<div style="padding:8px;font-size:12px;background:#e8f4f8;border-radius:4px;border-left:3px solid #17a2b8;">' +
                    fields.join('<br>') + '</div>';

            case 'text':
                var style = 'padding:8px;font-size:' + (p.fontSize || 13) + 'px;text-align:' + (p.align || 'left') + ';';
                if (p.bold) style += 'font-weight:bold;';
                return '<div style="' + style + '">' + escHtml(p.content || '') + '</div>';

            case 'table':
                return '<div style="padding:4px;"><table style="width:100%;border-collapse:collapse;font-size:11px;">' +
                    '<thead><tr style="background:' + (p.headerBg || '#2c3e50') + ';color:' + (p.headerColor || '#fff') + ';">' +
                    '<th style="padding:6px;border:1px solid #ddd;">#</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Poz</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Ürün</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">En</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Boy</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">m²</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Adet</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Fiyat</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Toplam</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Özellik</th>' +
                    '<th style="padding:6px;border:1px solid #ddd;">Not</th>' +
                    '</tr></thead><tbody><tr><td colspan="11" style="padding:8px;text-align:center;color:#999;border:1px solid #ddd;">{{Kalemler}}</td></tr></tbody></table></div>';

            case 'totals':
                return '<div style="text-align:' + (p.align || 'right') + ';padding:8px;">' +
                    '<table style="margin-left:auto;font-size:12px;"><tr><td style="padding:2px 8px;">Toplam Adet:</td><td><strong>{{ToplamAdet}}</strong></td></tr>' +
                    '<tr><td style="padding:2px 8px;">Toplam Alan:</td><td><strong>{{ToplamAlan}} m²</strong></td></tr>' +
                    '<tr><td style="padding:2px 8px;border-top:2px solid #333;">Genel Toplam:</td><td style="border-top:2px solid #333;"><strong>{{ToplamTutar}} {{ParaBirimi}}</strong></td></tr></table></div>';

            case 'signatures':
                var labels = (p.labels || 'Teslim Eden,Teslim Alan').split(',');
                var cols = '<div style="display:flex;justify-content:space-around;padding:8px;">';
                labels.forEach(function (l) {
                    cols += '<div style="text-align:center;min-width:120px;"><div style="border-bottom:1px solid #333;margin-bottom:4px;height:40px;"></div><small>' + escHtml(l.trim()) + '</small></div>';
                });
                return cols + '</div>';

            case 'divider':
                return '<hr style="border:none;border-top:1px ' + (p.style || 'solid') + ' ' + (p.color || '#dee2e6') + ';margin:8px 0;">';

            default:
                return '<div class="text-muted p-2">Bilinmeyen blok</div>';
        }
    }

    // Blok → HTML çıktısı (yazdırma şablonu için)
    function blockToHtml(block) {
        var p = block.props;
        switch (block.type) {
            case 'header':
                return '<div style="text-align:' + (p.align || 'center') + '; background:' + (p.bgColor || '#2c3e50') +
                    '; color:' + (p.textColor || '#fff') + '; padding:20px 30px; margin-bottom:20px;">\n' +
                    '  <div style="font-size:24px; font-weight:bold;">' + (p.title || '') + '</div>\n' +
                    '  <div style="font-size:14px; opacity:0.9; margin-top:4px;">' + (p.subtitle || '') + '</div>\n</div>\n';

            case 'docinfo':
                return '<div style="display:flex; justify-content:space-between; padding:10px 0; margin-bottom:15px; border-bottom:1px solid #ddd; font-size:13px;">\n' +
                    '  <div><strong>Belge No:</strong> {{SiparisNo}}</div>\n' +
                    '  <div><strong>Tarih:</strong> {{SiparisTarihi}}</div>\n' +
                    '  <div><strong>Teslim:</strong> {{TeslimTarihi}}</div>\n' +
                    '  <div><strong>Durum:</strong> {{Durum}}</div>\n' +
                    '  <div><strong>Para Birimi:</strong> {{ParaBirimi}}</div>\n</div>\n';

            case 'customerinfo':
                var lines = ['<div style="padding:12px; margin-bottom:15px; background:#f8f9fa; border-left:3px solid #17a2b8; font-size:13px;">'];
                lines.push('  <div style="font-size:15px; font-weight:bold; margin-bottom:6px;">{{MusteriAdi}}</div>');
                if (p.showAddress) lines.push('  <div>{{MusteriAdres}} / {{MusteriSehir}}</div>');
                if (p.showPhone) lines.push('  <div>Tel: {{MusteriTelefon}}</div>');
                if (p.showEmail) lines.push('  <div>E-posta: {{MusteriEmail}}</div>');
                if (p.showTax) lines.push('  <div>VKN: {{MusteriVergiNo}} - {{MusteriVergiDairesi}}</div>');
                lines.push('</div>');
                return lines.join('\n') + '\n';

            case 'text':
                var style = 'font-size:' + (p.fontSize || 13) + 'px; text-align:' + (p.align || 'left') + ';';
                if (p.bold) style += ' font-weight:bold;';
                return '<div style="' + style + ' padding:8px 0; margin-bottom:10px;">' + (p.content || '') + '</div>\n';

            case 'table':
                return '<table style="width:100%; border-collapse:collapse; margin-bottom:15px;">\n' +
                    '  <thead>\n    <tr style="background:' + (p.headerBg || '#2c3e50') + '; color:' + (p.headerColor || '#fff') + ';">\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:center; width:35px;">#</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd;">Poz No</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd;">Ürün / Reçete</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:right;">En (mm)</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:right;">Boy (mm)</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:right;">Alan (m²)</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:center;">Adet</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:right;">Birim Fiyat</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd; text-align:right;">Toplam</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd;">Özellikler</th>\n' +
                    '      <th style="padding:8px 10px; border:1px solid #ddd;">Not</th>\n' +
                    '    </tr>\n  </thead>\n  <tbody>\n    {{Kalemler}}\n  </tbody>\n</table>\n';

            case 'totals':
                return '<div style="text-align:' + (p.align || 'right') + '; margin-bottom:20px;">\n' +
                    '  <table style="margin-left:auto;">\n' +
                    '    <tr><td style="padding:4px 12px;">Toplam Adet:</td><td style="text-align:right;"><strong>{{ToplamAdet}}</strong></td></tr>\n' +
                    '    <tr><td style="padding:4px 12px;">Toplam Alan:</td><td style="text-align:right;"><strong>{{ToplamAlan}} m²</strong></td></tr>\n' +
                    '    <tr><td style="padding:4px 12px; border-top:2px solid #333;">Genel Toplam:</td><td style="text-align:right; border-top:2px solid #333;"><strong>{{ToplamTutar}} {{ParaBirimi}}</strong></td></tr>\n' +
                    '  </table>\n</div>\n';

            case 'signatures':
                var labels = (p.labels || 'Teslim Eden,Teslim Alan').split(',');
                var html = '<div style="display:flex; justify-content:space-around; margin-top:40px; padding-top:20px;">\n';
                labels.forEach(function (l) {
                    html += '  <div style="text-align:center; min-width:150px;">\n    <div style="border-bottom:1px solid #333; margin-bottom:6px; height:50px;"></div>\n    <div>' + l.trim() + '</div>\n  </div>\n';
                });
                return html + '</div>\n';

            case 'divider':
                return '<hr style="border:none; border-top:1px ' + (p.style || 'solid') + ' ' + (p.color || '#dee2e6') + '; margin:15px 0;">\n';

            default:
                return '';
        }
    }

    // Tüm blokları HTML'e derle
    function compileToHtml() {
        var css = '<style>\n  @page { size: A4; margin: 15mm; }\n  body { font-family: Arial, sans-serif; font-size: 13px; color: #333; }\n' +
            '  table { page-break-inside: auto; }\n  tr { page-break-inside: avoid; }\n' +
            '  @media print { body { padding: 0; } }\n</style>\n\n';
        var body = '';
        blocks.forEach(function (b) { body += blockToHtml(b); });
        return css + body;
    }

    function syncToTextarea() {
        var html = compileToHtml();
        $('#HtmlContent').val(html);
        // Önizlemeyi güncelle
        refreshPreview();
    }

    function refreshPreview() {
        var html = $('#HtmlContent').val();
        var frame = document.getElementById('previewFrame');
        if (frame) {
            var doc = frame.contentDocument || frame.contentWindow.document;
            doc.open();
            doc.write(html);
            doc.close();
        }
    }

    // Özellik panelini render et
    function renderProperties() {
        var $panel = $('#veProperties');
        if (!activeBlockId) {
            $panel.html('<div class="text-center text-muted py-4"><i class="fas fa-hand-pointer fa-2x mb-2" style="opacity:0.3"></i><p class="small">Bir bloğa tıklayarak<br>özelliklerini düzenleyin</p></div>');
            return;
        }

        var block = getBlock(activeBlockId);
        if (!block) return;

        var bt = blockTypes[block.type];
        var p = block.props;
        var html = '<div class="ve-prop-header"><i class="' + bt.icon + ' mr-1"></i> ' + bt.label + '</div>';

        switch (block.type) {
            case 'header':
                html += propInput('Başlık', 'title', p.title);
                html += propInput('Alt Başlık', 'subtitle', p.subtitle, 'Placeholder kullanabilirsiniz: {{SiparisNo}}');
                html += propSelect('Hizalama', 'align', p.align, { left: 'Sol', center: 'Orta', right: 'Sağ' });
                html += propColor('Arkaplan', 'bgColor', p.bgColor);
                html += propColor('Yazı Rengi', 'textColor', p.textColor);
                break;

            case 'docinfo':
                html += '<p class="small text-muted">Bu blok otomatik olarak belge bilgilerini gösterir.</p>';
                break;

            case 'customerinfo':
                html += propCheck('Adres Göster', 'showAddress', p.showAddress);
                html += propCheck('Telefon Göster', 'showPhone', p.showPhone);
                html += propCheck('E-posta Göster', 'showEmail', p.showEmail);
                html += propCheck('Vergi Bilgisi Göster', 'showTax', p.showTax);
                break;

            case 'text':
                html += propTextarea('İçerik', 'content', p.content);
                html += propSelect('Hizalama', 'align', p.align, { left: 'Sol', center: 'Orta', right: 'Sağ' });
                html += propInput('Yazı Boyutu (px)', 'fontSize', p.fontSize);
                html += propCheck('Kalın', 'bold', p.bold);
                break;

            case 'table':
                html += propColor('Başlık Arkaplan', 'headerBg', p.headerBg);
                html += propColor('Başlık Yazı', 'headerColor', p.headerColor);
                break;

            case 'totals':
                html += propSelect('Hizalama', 'align', p.align, { left: 'Sol', center: 'Orta', right: 'Sağ' });
                break;

            case 'signatures':
                html += propInput('İmza Etiketleri', 'labels', p.labels, 'Virgülle ayırın: Teslim Eden,Teslim Alan');
                break;

            case 'divider':
                html += propSelect('Çizgi Stili', 'style', p.style, { solid: 'Düz', dashed: 'Kesikli', dotted: 'Noktalı', double: 'Çift' });
                html += propColor('Renk', 'color', p.color);
                break;
        }

        $panel.html(html);
    }

    // Yardımcı: form bileşenleri
    function propInput(label, key, value, hint) {
        return '<div class="form-group mb-2"><label class="small mb-0">' + label + '</label>' +
            '<input type="text" class="form-control form-control-sm ve-prop-input" data-key="' + key + '" value="' + escAttr(value || '') + '">' +
            (hint ? '<small class="text-muted">' + hint + '</small>' : '') + '</div>';
    }

    function propTextarea(label, key, value) {
        return '<div class="form-group mb-2"><label class="small mb-0">' + label + '</label>' +
            '<textarea class="form-control form-control-sm ve-prop-input" data-key="' + key + '" rows="3">' + escHtml(value || '') + '</textarea></div>';
    }

    function propSelect(label, key, value, options) {
        var html = '<div class="form-group mb-2"><label class="small mb-0">' + label + '</label><select class="form-control form-control-sm ve-prop-input" data-key="' + key + '">';
        for (var k in options) {
            html += '<option value="' + k + '"' + (value === k ? ' selected' : '') + '>' + options[k] + '</option>';
        }
        return html + '</select></div>';
    }

    function propColor(label, key, value) {
        return '<div class="form-group mb-2"><label class="small mb-0">' + label + '</label>' +
            '<div class="input-group input-group-sm">' +
            '<input type="color" class="form-control form-control-sm ve-prop-input" data-key="' + key + '" value="' + (value || '#000000') + '" style="max-width:50px;padding:2px;">' +
            '<input type="text" class="form-control form-control-sm ve-prop-input" data-key="' + key + '" value="' + escAttr(value || '') + '">' +
            '</div></div>';
    }

    function propCheck(label, key, value) {
        return '<div class="custom-control custom-switch mb-2">' +
            '<input type="checkbox" class="custom-control-input ve-prop-check" id="vep_' + key + '" data-key="' + key + '"' + (value ? ' checked' : '') + '>' +
            '<label class="custom-control-label small" for="vep_' + key + '">' + label + '</label></div>';
    }

    function escHtml(s) { return $('<div>').text(s).html(); }
    function escAttr(s) { return s.replace(/"/g, '&quot;'); }

    // Event binding
    function bindEvents() {
        // Blok palette tıklama
        $(document).on('click', '.ve-palette-item', function (e) {
            e.preventDefault();
            var type = $(this).data('type');
            addBlock(type);
        });

        // Canvas'ta blok seçimi
        $(document).on('click', '.ve-block', function (e) {
            if ($(e.target).closest('.ve-btn-remove,.ve-btn-clone,.ve-btn-edit').length) return;
            selectBlock($(this).data('id'));
        });

        // Blok sil
        $(document).on('click', '.ve-btn-remove', function (e) {
            e.stopPropagation();
            var id = $(this).closest('.ve-block').data('id');
            removeBlock(id);
        });

        // Blok kopyala
        $(document).on('click', '.ve-btn-clone', function (e) {
            e.stopPropagation();
            var $block = $(this).closest('.ve-block');
            var id = $block.data('id');
            var block = getBlock(id);
            if (block) addBlock(block.type, $.extend({}, block.props), id);
        });

        // Blok düzenle
        $(document).on('click', '.ve-btn-edit', function (e) {
            e.stopPropagation();
            selectBlock($(this).closest('.ve-block').data('id'));
        });

        // Özellik değişikliği - input/select
        $(document).on('input change', '.ve-prop-input', function () {
            if (!activeBlockId) return;
            var key = $(this).data('key');
            var val = $(this).val();
            // Senkronize renk input'ları
            if ($(this).attr('type') === 'color') {
                $(this).siblings('.ve-prop-input[data-key="' + key + '"]').val(val);
            } else if ($(this).siblings('input[type="color"]').length) {
                $(this).siblings('input[type="color"]').val(val);
            }
            updateBlockProp(activeBlockId, key, val);
        });

        // Özellik değişikliği - checkbox
        $(document).on('change', '.ve-prop-check', function () {
            if (!activeBlockId) return;
            updateBlockProp(activeBlockId, $(this).data('key'), $(this).is(':checked'));
        });

        // Tab geçişi
        $(document).on('click', '#veTabVisual', function () {
            $('#veTabVisual').addClass('active');
            $('#veTabHtml').removeClass('active');
            $('#visualEditorPanel').show();
            $('#htmlEditorPanel').hide();
            // HTML'den blokları yeniden parse etmiyoruz, mevcut blokları göster
            renderCanvas();
        });

        $(document).on('click', '#veTabHtml', function () {
            $('#veTabHtml').addClass('active');
            $('#veTabVisual').removeClass('active');
            $('#htmlEditorPanel').show();
            $('#visualEditorPanel').hide();
        });
    }

    // Palette (bileşen paneli) render
    function renderPalette() {
        var html = '';
        for (var type in blockTypes) {
            var bt = blockTypes[type];
            html += '<div class="ve-palette-item" data-type="' + type + '" draggable="true">' +
                '<i class="' + bt.icon + '" style="color:' + bt.color + '"></i>' +
                '<span>' + bt.label + '</span></div>';
        }
        $('#vePalette').html(html);
    }

    // Hızlı şablon oluştur (tek tuş)
    function quickCreate(outputType) {
        blocks = [];
        blockIdCounter = 0;

        var type = parseInt(outputType);
        if (type <= 1) {
            // Sipariş / Teslimat
            addBlock('header', { title: 'GLASSOFT', subtitle: type === 0 ? 'SİPARİŞ FİŞİ' : 'TESLİMAT İRSALİYESİ' });
            addBlock('docinfo');
            addBlock('customerinfo');
            addBlock('divider');
            addBlock('table');
            addBlock('totals');
            addBlock('divider');
            if (type === 1) addBlock('signatures', { labels: 'Teslim Eden,Taşıyan,Teslim Alan' });
            else addBlock('signatures', { labels: 'Düzenleyen,Onaylayan' });
            addBlock('text', { content: '{{Notlar}}', fontSize: '11', align: 'left' });
        } else if (type === 2) {
            // Üretim
            addBlock('header', { title: 'GLASSOFT', subtitle: 'ÜRETİM EMRİ - {{IsEmriNo}}', bgColor: '#1a5276' });
            addBlock('text', { content: '<strong>İş Emri Tarihi:</strong> {{IsEmriTarihi}} | <strong>Durum:</strong> {{Durum}} | <strong>Müşteri:</strong> {{MusteriAdi}}', fontSize: '12' });
            addBlock('text', { content: '<strong>Sipariş Noları:</strong> {{SiparisNolari}}', fontSize: '12' });
            addBlock('divider');
            addBlock('table', { headerBg: '#1a5276' });
            addBlock('totals');
            addBlock('signatures', { labels: 'Hazırlayan,Üretim Sorumlusu' });
        } else {
            // Cari
            addBlock('header', { title: 'GLASSOFT', subtitle: type === 3 ? 'CARİ HAREKET RAPORU' : 'CARİ HESAP EKSTRESİ', bgColor: '#1a472a' });
            addBlock('customerinfo');
            addBlock('text', { content: '<strong>Dönem:</strong> {{BaslangicTarihi}} - {{BitisTarihi}}', fontSize: '12' });
            if (type === 4) addBlock('text', { content: '<strong>Devreden Bakiye:</strong> {{DevredenBakiye}} TL ({{DevredenYon}})', fontSize: '12', bold: true });
            addBlock('divider');
            addBlock('text', { content: '{{Hareketler}}', fontSize: '12' });
            addBlock('divider');
            addBlock('text', { content: '<strong>Toplam Borç:</strong> {{ToplamBorc}} TL | <strong>Toplam Alacak:</strong> {{ToplamAlacak}} TL | <strong>Bakiye:</strong> {{Bakiye}} TL ({{BakiyeYon}})', fontSize: '13', bold: true });
        }

        renderCanvas();
        syncToTextarea();
    }

    // Public API
    return {
        init: function () {
            renderPalette();
            renderCanvas();
            renderProperties();
            bindEvents();
        },
        addBlock: addBlock,
        quickCreate: quickCreate,
        getBlocks: function () { return blocks; },
        refreshPreview: refreshPreview
    };
})();
