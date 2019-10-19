(function ($) {
    
    var kendo = window.kendo;

    var DecimalBox = kendo.ui.Widget.extend({
        init: function (element, options) {
            kendo.ui.Widget.fn.init.call(this, element, options);

            var settings = options.settings === 'undefined' ? this.options.settings : options.settings;
            $(this.element).autoNumeric('init',settings);

            this._changeHandler = $.proxy(this._change, this);
            this.element.on("change", this._changeHandler);
            this.element.on("focus", this._focus);
        },

        options: {
            name: "DecimalBox",
            settings: { mDec :"0",aSep:","}
        },

        _change: function () {
            //this._value = this.element.val();
            this._value = $(this.element).autoNumeric('get');
            this.trigger("change");
        },

        _focus: function () {
            //this.select();
        },

        value: function (value) {
            if (value !== undefined) {
                //this.element.val(value);
                $(this.element).autoNumeric('set',value);
            } else {
                //return this.element.val();
                return $(this.element).autoNumeric('get');
            }
        },

        destroy: function () {
            this.element.off("change", this._changeHandler);
            this.element.off("focus", this._focus);
        }
    });

    kendo.ui.plugin(DecimalBox);   

})(jQuery);

ko.kendo.bindingFactory.createBinding({
    name: "kendoDecimalBox",
    defaultOption: "value",
    //events to bind against. update the view model based on interactions with the widget
    events: {
        change: "value"        
    },
    //observables to watch. react to changes by calling methods on the widget
    watch: {
        value: "value"
    }
});

