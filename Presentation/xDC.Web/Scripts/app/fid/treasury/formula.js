var formula = (function () {
    var _formula = {};

    _formula.tenor = function (maturityDate, valueDate) {
        return moment(maturityDate).diff(valueDate, "days");
    };

    _formula.inflow_proceeds = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        var calcIntDividendReceivable = 0;
        var proceeds = 0;

        if (productType == "NID") {
            calcIntDividendReceivable =
                _formula.inflow_intDividendReceivable(productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds);
            proceeds = calcIntDividendReceivable + nominal;
            return proceeds;

        } else {
            proceeds = Math.round(nominal - (nominal * tenor / 36500 * purchaseRate));
            return proceeds;

        }
    };

    _formula.inflow_intDividendReceivable = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        var proceeds = 0;
        var intDividendReceivable = 0;

        if (productType == "NID") {
            intDividendReceivable = Math.round(purchaseRate * tenor * nominal / 36500);
            return intDividendReceivable;
        } else {
            proceeds = _formula.inflow_proceeds(productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds);
            intDividendReceivable = Math.round(proceeds - purchaseProceeds);
            return intDividendReceivable;
        }
    };

    _formula.inflow_price = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        var calcIntDividendReceivable = 0;
        var proceeds = 0;
        var price = 0;

        if (productType == "NID") {
            proceeds = _formula.inflow_proceeds(productType,
                maturityDate,
                valueDate,
                nominal,
                purchaseRate,
                purchaseProceeds);

            price = Math.round(proceeds / nominal);
            return price;

        } else {
            proceeds = _formula.inflow_proceeds(productType,
                maturityDate,
                valueDate,
                nominal,
                purchaseRate,
                purchaseProceeds);
            price = Math.round(proceeds / nominal);
            return price;
        }
    };

    _formula.outflow_proceeds = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        var calcIntDividendReceivable = 0;
        var price = 0;
        var proceeds = 0;

        if (productType == "NID" || productType == "NIDL") {
            proceeds = nominal;
            return proceeds;
        }
        else if (productType == "NIDC") {
            price = _formula.outflow_price(productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds);
            proceeds = Math.round(nominal * price / 100);
            return proceeds;
        }
        else {
            proceeds = Math.round(nominal - (purchaseRate * tenor / 36500 * nominal));
            return proceeds;
        }
    };

    _formula.outflow_intDividendReceivable = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        var proceeds = 0;
        var intDividendReceivable = 0;

        if (productType == "NID" || productType == "NIDL") {
            intDividendReceivable = Math.round(purchaseRate * tenor * nominal / 36500);
            return intDividendReceivable;
        } else {
            proceeds = _formula.outflow_proceeds(productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds);
            intDividendReceivable = Math.round(nominal - proceeds);
            return intDividendReceivable;
        }
    };

    _formula.outflow_price = function (productType, maturityDate, valueDate, nominal, purchaseRate, purchaseProceeds) {
        var tenor = _formula.tenor(maturityDate, valueDate);
        //var calcIntDividendReceivable = 0;
        var proceeds = 0;
        var price = 0;

        if (productType == "NIDC") {
            proceeds = _formula.outflow_proceeds(productType,
                maturityDate,
                valueDate,
                nominal,
                purchaseRate,
                purchaseProceeds);

            price = Math.round(proceeds / nominal);
            return price;

        } else {
            proceeds = _formula.outflow_proceeds(productType,
                maturityDate,
                valueDate,
                nominal,
                purchaseRate,
                purchaseProceeds);
            price = Math.round(proceeds / nominal);
            return price;
        }
    };

    return _formula;
}());