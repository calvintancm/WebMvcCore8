/**
 * sweetalert-kendo.js
 * Fixes SweetAlert2 z-index conflict with KendoUI Windows (2018+)
 *
 * Usage: replace all Swal.fire() calls with KSwal.fire()
 * 
 *   KSwal.kendoAlert("Something went wrong.", "Error", "error");
 *   KSwal.fire("Saved!", "Record updated successfully.", "success");
 */

var KSwal = (function () {

   
    var SWAL_ACTIVE_ZINDEX = 19999;
    var SWAL_DEFAULT_ZINDEX = 1060;

 
    function suppressKendoWindows() {
        var saved = [];

        $(".k-window").each(function () {
            var $win = $(this);
            saved.push({
                el: $win,
                z: $win.css("z-index")
            });
            $win.css("z-index", SWAL_DEFAULT_ZINDEX - 1);   // push behind Swal
        });

        return saved;
    }

 
    function restoreKendoWindows(saved) {
        if (!saved) return;
        $.each(saved, function (i, item) {
            item.el.css("z-index", item.z);
        });
    }

   
    function fire(options) {

      
        if (typeof options === "string") {
            options = {
                title: arguments[0],
                text: arguments[1] || "",
                icon: arguments[2] || "info"
            };
        }

        var savedWindows = null;

        var fixedOptions = $.extend({}, options, {

       
            willOpen: function (popup) {
                savedWindows = suppressKendoWindows();

                var container = popup.closest(".swal2-container");
                if (container) {
                    container.style.zIndex = SWAL_ACTIVE_ZINDEX;
                }

              
                if (typeof options.willOpen === "function") {
                    options.willOpen(popup);
                }
            },

       
            didClose: function () {
                restoreKendoWindows(savedWindows);
                savedWindows = null;

           
                if (!Swal.isVisible()) {
                    $(".swal2-container").css("z-index", SWAL_DEFAULT_ZINDEX);
                }

                if (typeof options.didClose === "function") {
                    options.didClose();
                }
            }
        });

        return Swal.fire(fixedOptions);
    }

    function kendoAlert(message, title, icon) {
        return fire({
            title: title || "Notice",
            text: message || "",
            icon: icon || "info",
            confirmButtonText: "OK"
        });
    }


    return {
        fire: fire,
        kendoAlert: kendoAlert,
        suppressKendoWindows: suppressKendoWindows,
        restoreKendoWindows: restoreKendoWindows
    };

}());