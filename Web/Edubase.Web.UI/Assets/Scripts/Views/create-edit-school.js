﻿DfE = window.DfE || {};

DfE.Views.createEdit = (function() {

    $('.help-icon').on('click', function(e) {
        e.preventDefault();
        $(this).next('.help-text').toggle();
    });

    function processLAESTABTextBox() {
        var $input = $("#laestab");
        var p = $("#educationphaseid").val();
        var t = $("#typeid").val();
        if (!p) p = "";
        if (!t) t = "";
        var c = p + "-" + t;
        var rule = laestabRules[c];
        if (rule) {
            if (rule == "UserDefined") {
                $input.removeProp("disabled");
                $input.prop("placeholder", "Input an LAESTAB no.");

            } else if (rule == "SystemGenerated") {
                $input.val("")
                    .attr("disabled", "disabled")
                    .prop("placeholder", "autogenerated");

            } else if (rule == "NonePermitted") {
                $input.val("")
                    .attr("disabled", 'disabled')
                    .prop("placeholder", "n/a");
            }
        }

        $input.parent().removeClass("error");
    }

    $("#educationphaseid,#typeid").change(processLAESTABTextBox);


}());