/**
 * sweetalert-kendo.js  — PTC IGH System
 * ═══════════════════════════════════════════════════════════════
 * Fixes SweetAlert2 z-index conflict with KendoUI 2018 + Bootstrap 5
 * Ensures SweetAlert2 always appears TRUE CENTER of viewport
 *
 * GLOBAL USAGE:
 *   KSwal.toast('success'|'error'|'info'|'warning', 'Message')
 *   KSwal.alert('Message', 'Title', 'info')
 *   KSwal.success('Title', 'Message')
 *   KSwal.error('Title', 'Message')
 *   KSwal.warning('Title', 'Message')
 *   KSwal.confirm('Title', 'Message', onOk, onCancel)
 *   KSwal.confirmDelete('Title', 'Message', onOk)
 *   KSwal.loading('Please wait...')
 *   KSwal.close()
 *   KSwal.fire({ title:'Done', text:'Saved!', icon:'success' })
 *   KSwal.fire('Title', 'Message', 'success')
 * ═══════════════════════════════════════════════════════════════
 */

var KSwal = (function () {
    'use strict';

    /* ── Z-Index constants ──────────────────────────────────── */
    var Z = {
        SWAL_ACTIVE:  20000,
        SWAL_DEFAULT: 1060
    };

    /* ── Inject CSS once on load ────────────────────────────── */
    (function injectStyles() {
        if (document.getElementById('kswal-styles')) return;
        var style = document.createElement('style');
        style.id = 'kswal-styles';
        /* NOTE: plain .css file so single @ is correct here */
        style.innerHTML = [
            '.swal2-container {',
            '  position: fixed !important;',
            '  top: 0 !important;',
            '  left: 0 !important;',
            '  right: 0 !important;',
            '  bottom: 0 !important;',
            '  width: 100vw !important;',
            '  height: 100vh !important;',
            '  display: flex !important;',
            '  align-items: center !important;',
            '  justify-content: center !important;',
            '  margin: 0 !important;',
            '  padding: 20px !important;',
            '  z-index: ' + Z.SWAL_DEFAULT + ' !important;',
            '  pointer-events: all !important;',
            '}',
            '.swal2-popup {',
            '  position: relative !important;',
            '  margin: auto !important;',
            '  width: 440px !important;',
            '  max-width: calc(100vw - 40px) !important;',
            '  border-radius: 14px !important;',
            '  font-family: "DM Sans", sans-serif !important;',
            '  box-shadow: 0 24px 60px rgba(0,0,0,0.25) !important;',
            '}',
            '.swal2-container.swal2-center { align-items: center !important; }',
            '.swal2-container.swal2-top    { align-items: flex-start !important; padding-top: 16px !important; }',
            '.swal2-popup.swal2-toast {',
            '  width: auto !important;',
            '  min-width: 260px !important;',
            '  max-width: calc(100vw - 40px) !important;',
            '  border-radius: 10px !important;',
            '}',
            '.swal2-title {',
            '  font-family: "Sora", sans-serif !important;',
            '  font-size: 1.15rem !important;',
            '  color: #1e293b !important;',
            '  padding-top: 6px !important;',
            '}',
            '.swal2-html-container {',
            '  font-size: 0.875rem !important;',
            '  color: #475569 !important;',
            '  line-height: 1.65 !important;',
            '}',
            '.swal2-actions { gap: 10px !important; margin-top: 20px !important; }',
            '.swal2-confirm, .swal2-cancel, .swal2-deny {',
            '  border-radius: 8px !important;',
            '  font-family: "DM Sans", sans-serif !important;',
            '  font-weight: 600 !important;',
            '  font-size: 0.875rem !important;',
            '  padding: 9px 22px !important;',
            '  min-width: 90px !important;',
            '}'
        ].join('\n');
        document.head.appendChild(style);
    })();

    /* ── Suppress Kendo + Bootstrap layers ─────────────────── */
    function suppressKendoWindows() {
        var saved = [];
        if (typeof $ === 'undefined') return saved;

        $('.k-window').each(function () {
            var $w = $(this);
            saved.push({ el: $w, z: $w.css('z-index') });
            $w.css('z-index', Z.SWAL_DEFAULT - 1);
        });

        $('.modal.show').each(function () {
            var $m = $(this), $b = $('.modal-backdrop');
            saved.push({ el: $m, z: $m.css('z-index') });
            saved.push({ el: $b, z: $b.css('z-index') });
            $m.css('z-index', Z.SWAL_DEFAULT - 2);
            $b.css('z-index', Z.SWAL_DEFAULT - 3);
        });

        return saved;
    }

    function restoreKendoWindows(saved) {
        if (!saved || !saved.length) return;
        $.each(saved, function (i, item) {
            if (item.el && item.el.length) item.el.css('z-index', item.z);
        });
    }

    /* ── Elevate container to true viewport center ──────────── */
    function elevateContainer(popup) {
        var container = null;
        if (popup && popup.closest) {
            container = popup.closest('.swal2-container');
        }
        if (!container && typeof $ !== 'undefined') {
            container = $('.swal2-container')[0];
        }
        if (container) {
            container.style.setProperty('z-index',  Z.SWAL_ACTIVE, 'important');
            container.style.setProperty('position', 'fixed',       'important');
            container.style.setProperty('top',      '0',           'important');
            container.style.setProperty('left',     '0',           'important');
        }
    }

    /* ── Core fire() ────────────────────────────────────────── */
    function fire(options, text, icon) {
        if (typeof options === 'string') {
            options = { title: options, text: text || '', icon: icon || 'info' };
        }

        var savedWindows   = null;
        var callerWillOpen = options.willOpen;
        var callerDidClose = options.didClose;

        var merged = $.extend({}, options, {
            willOpen: function (popup) {
                savedWindows = suppressKendoWindows();
                elevateContainer(popup);
                if (typeof callerWillOpen === 'function') callerWillOpen(popup);
            },
            didClose: function () {
                restoreKendoWindows(savedWindows);
                savedWindows = null;
                if (!Swal.isVisible()) {
                    $('.swal2-container').css('z-index', Z.SWAL_DEFAULT);
                }
                if (typeof callerDidClose === 'function') callerDidClose();
            }
        });

        return Swal.fire(merged);
    }

    /* ── toast() ────────────────────────────────────────────── */
    function toast(icon, message, timer) {
        var saved = suppressKendoWindows();
        return Swal.mixin({
            toast:             true,
            position:          'center',
            showConfirmButton: false,
            timer:             timer || 3200,
            timerProgressBar:  true,
            didOpen: function (t) {
                elevateContainer(t);
                t.addEventListener('mouseenter', Swal.stopTimer);
                t.addEventListener('mouseleave', Swal.resumeTimer);
            },
            didClose: function () {
                restoreKendoWindows(saved);
                if (!Swal.isVisible()) {
                    $('.swal2-container').css('z-index', Z.SWAL_DEFAULT);
                }
            }
        }).fire({ icon: icon, title: message });
    }

    /* ── alert() ────────────────────────────────────────────── */
    function alert(message, title, icon) {
        return fire({
            title:              title   || 'Notice',
            html:               message || '',
            icon:               icon    || 'info',
            confirmButtonText:  'OK',
            confirmButtonColor: '#1a6eb5'
        });
    }

    /* kendoAlert kept for backward compatibility */
    function kendoAlert(message, title, icon) { return alert(message, title, icon); }

    /* ── success() ──────────────────────────────────────────── */
    function success(title, message) {
        return fire({
            title:              title   || 'Success',
            html:               message || '',
            icon:               'success',
            confirmButtonText:  'OK',
            confirmButtonColor: '#059669'
        });
    }

    /* ── error() ────────────────────────────────────────────── */
    function error(title, message) {
        return fire({
            title:              title   || 'Error',
            html:               message || '',
            icon:               'error',
            confirmButtonText:  'OK',
            confirmButtonColor: '#dc2626'
        });
    }

    /* ── warning() ──────────────────────────────────────────── */
    function warning(title, message) {
        return fire({
            title:              title   || 'Warning',
            html:               message || '',
            icon:               'warning',
            confirmButtonText:  'OK',
            confirmButtonColor: '#d97706'
        });
    }

    /* ── confirm() ──────────────────────────────────────────── */
    function confirm(title, message, onConfirm, onCancel) {
        return fire({
            title:              title   || 'Are you sure?',
            html:               message || '',
            icon:               'question',
            showCancelButton:   true,
            confirmButtonText:  'Confirm',
            cancelButtonText:   'Cancel',
            confirmButtonColor: '#1a6eb5',
            cancelButtonColor:  '#94a3b8',
            reverseButtons:     true
        }).then(function (r) {
            if (r.isConfirmed && typeof onConfirm === 'function') onConfirm();
            if (r.isDismissed && typeof onCancel  === 'function') onCancel();
        });
    }

    /* ── confirmDelete() ────────────────────────────────────── */
    function confirmDelete(title, message, onConfirm) {
        return fire({
            title:              title   || 'Delete?',
            html:               message || 'This action cannot be undone.',
            icon:               'warning',
            showCancelButton:   true,
            confirmButtonText:  'Delete',
            cancelButtonText:   'Cancel',
            confirmButtonColor: '#dc2626',
            cancelButtonColor:  '#94a3b8',
            reverseButtons:     true
        }).then(function (r) {
            if (r.isConfirmed && typeof onConfirm === 'function') onConfirm();
        });
    }

    /* ── loading() ──────────────────────────────────────────── */
    function loading(message) {
        var saved = suppressKendoWindows();
        Swal.fire({
            title:             message || 'Please wait...',
            allowOutsideClick: false,
            allowEscapeKey:    false,
            showConfirmButton: false,
            willOpen: function (popup) {
                Swal.showLoading();
                elevateContainer(popup);
            },
            didClose: function () { restoreKendoWindows(saved); }
        });
    }

    /* ── close() ────────────────────────────────────────────── */
    function close() { Swal.close(); }

    /* ── Public API ─────────────────────────────────────────── */
    return {
        fire:                 fire,
        toast:                toast,
        alert:                alert,
        kendoAlert:           kendoAlert,
        success:              success,
        error:                error,
        warning:              warning,
        confirm:              confirm,
        confirmDelete:        confirmDelete,
        loading:              loading,
        close:                close,
        suppressKendoWindows: suppressKendoWindows,
        restoreKendoWindows:  restoreKendoWindows
    };

}());
