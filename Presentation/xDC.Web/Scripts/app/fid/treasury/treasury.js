var treasury = (function () {
    var _treasury = {};

    _treasury.tenor = function (maturityDate, valueDate) {
        return moment(maturityDate).diff(valueDate, "days");
    };

    _treasury.outflow_proceeds = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            result = nominal;
            return result;

        case "NIDC":
            var price = (100 / (1 + (tenor * rate / 36500)));
            result = nominal * price / 100;
            return result;

        case "CP":
        case "ICP":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        case "BA":
        case "AB-i":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        case "NIDL":
            result = nominal;
            return result;

        default:
            return result;
        }
    }

    _treasury.outflow_intDiv = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            result = rate * tenor * nominal / 36500;
            return result;

        case "NIDC":
            var proceedsNidc = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsNidc;
            return result;

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsCp;
            return result;

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsBa;
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsOthers;
            return result;

        case "NIDL":
            result = rate * tenor * nominal / 36500;
            return result;

        default:
            return result;
        }
    }

    _treasury.outflow_price = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            var proceedsNid = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNid / nominal * 100;
            return result;

        case "NIDC":
            result = (100 / (1 + (tenor * rate / 36500)));
            return result;

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsCp / nominal * 100;
            return result;

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsBa / nominal * 100;
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsOthers / nominal * 100;
            return result;
        case "NIDL":
            var proceedsNidl = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNidl / nominal * 100;
            return result;

        default:
            return result;
        }
    }


    _treasury.inflow_proceeds = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            var interestDividendNid = rate * tenor * nominal / 36500;
            result = interestDividendNid + nominal;
            return result;

        case "NIDC":
            result = nominal - (nominal * tenor / 36500 * rate);
            return result;
            
        case "CP":
        case "ICP":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;
            
        case "BA":
        case "AB-i":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        case "NIDL":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result;

        default:
            return result;
        }
    }

    _treasury.inflow_intDiv = function (productType, nominal, rate, tenor, purchaseProceed) {
        var result = 0;

        switch (productType) {
        case "NID":
                result = rate * tenor * nominal / 36500;
            return result;

        case "NIDC":
            var proceedsNidc = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNidc - purchaseProceed;
            return result;

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsCp - purchaseProceed;
            return result;

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsBa - purchaseProceed;
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsOthers - purchaseProceed;
            return result;

        case "NIDL":
            var proceedsNidl = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNidl - purchaseProceed;
            return result;

        default:
            return result;
        }
    }

    _treasury.inflow_price = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            var proceedsNid = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNid / nominal) * 100;
            return result;

        case "NIDC":
            var proceedsNidc = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNidc / nominal) * 100;
            return result;

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsCp / nominal) * 100;
            return result;

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsBa / nominal) * 100;
            return result;

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsOthers / nominal) * 100;
            return result;

        case "NIDL":
            var proceedsNidl = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNidl / nominal) * 100;
            return result;

        default:
            return result;
        }
    }
    
    _treasury.dsNotes = ["w/d P+I", "r/o P+I", "New"];

    _treasury.dsAssetType = ["MMD", "FD", "CMD"];

    _treasury.dsProductType = ["NID", "NIDC", "NIDL", "CP", "ICP", "BA", "AB-i", "BNMN",
        "BNMN-i", "MTB", "MTIB"];


    return _treasury;
}());