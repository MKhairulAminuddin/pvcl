var treasury = (function () {
    var _treasury = {};

    _treasury.api = {
        loadNotes: window.location.origin + "/api/common/Treasury/Notes",
        loadAssetType: window.location.origin + "/api/common/Treasury/AssetType",
        loadProductType: window.location.origin + "/api/common/Treasury/ProductType",
        loadIssuer: window.location.origin + "/api/fid/Treasury/EdwIssuer/",
        loadCounterParty: window.location.origin + "/api/fid/Treasury/EdwBankCounterParty/",
    };

    _treasury.tenor = function (maturityDate, valueDate) {
        return moment(maturityDate).diff(valueDate, "days");
    };

    _treasury.outflow_depoInt = function(currency, principal, tenor, rate) {
        var result = 0;

        switch (currency) {
            case "USD":
            case "AUD":
            case "EUR":
                result = principal * tenor / 36000 * rate;
                return result.toFixed(2.5);
            default:
                result = principal * tenor / 36500 * rate;
                return result.toFixed(2.5);
        }
    }

    _treasury.outflow_depo_PrincipalInt = function (currency, principal, tenor, rate) {
        var result = 0;

        switch (currency) {
        case "USD":
        case "AUD":
        case "EUR":
            result = principal + (principal * tenor / 36000 * rate);
            return result.toFixed(2.5);
        default:
            result = principal + (principal * tenor / 36500 * rate);
            return result.toFixed(2.5);
        }
    }

    _treasury.outflow_proceeds = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            result = nominal;
            return result;

        case "NIDC":
            var price = (100 / (1 + (tenor * rate / 36500)));
            result = (nominal * price / 100);
            return result.toFixed(2.5);

        case "CP":
        case "ICP":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

        case "BA":
        case "AB-i":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

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
            result = (rate * tenor * nominal / 36500);
            return result.toFixed(2.5);

        case "NIDC":
            var proceedsNidc = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (nominal - proceedsNidc);
            return result.toFixed(2.5);

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsCp;
            return result.toFixed(2.5);

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsBa;
            return result.toFixed(2.5);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = nominal - proceedsOthers;
            return result.toFixed(2.5);

        case "NIDL":
            result = (rate * tenor * nominal / 36500);
            return result.toFixed(2.5);

        default:
            return result;
        }
    }

    _treasury.outflow_price = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            var proceedsNid = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNid / nominal * 100);
            return result.toFixed(2.5);

        case "NIDC":
            result = (100 / (1 + (tenor * rate / 36500)));
            return result.toFixed(4);

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsCp / nominal * 100);
            return result.toFixed(2.5);

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsBa / nominal * 100);
            return result.toFixed(2.5);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsOthers / nominal * 100);
            return result.toFixed(2.5);
        case "NIDL":
            var proceedsNidl = _treasury.outflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNidl / nominal * 100);
            return result.toFixed(2.5);

        default:
            return result;
        }
    }

    _treasury.inflow_proceeds = function (productType, nominal, rate, tenor) {
        var result = 0;

        switch (productType) {
        case "NID":
            var interestDividendNid = (rate * tenor * nominal / 36500);
            result = interestDividendNid + nominal;
            return result.toFixed(2.5);

        case "NIDC":
            var price = (100 / (1 + (tenor * rate / 36500)));
            result = (price * nominal / 100);
            return result.toFixed(2.5);
            
        case "CP":
        case "ICP":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);
            
        case "BA":
        case "AB-i":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

        case "NIDL":
            result = nominal - (rate * tenor / 36500 * nominal);
            return result.toFixed(2.5);

        default:
            return result;
        }
    }

    _treasury.inflow_intDiv = function (productType, nominal, rate, tenor, purchaseProceed) {
        var result = 0;

        switch (productType) {
        case "NID":
            result = (rate * tenor * nominal / 36500);
            return result.toFixed(2.5);

        case "NIDC":
            var proceedsNidc = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNidc - purchaseProceed;
            return result.toFixed(2.5);

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsCp - purchaseProceed;
            return result.toFixed(2.5);

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsBa - purchaseProceed;
            return result.toFixed(2.5);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsOthers - purchaseProceed;
            return result.toFixed(2.5);

        case "NIDL":
            var proceedsNidl = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = proceedsNidl - purchaseProceed;
            return result.toFixed(2.5);

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
            return result.toFixed(2.5);

        case "NIDC":
            result = (100 / (1 + (tenor * rate / 36500)));
            return result.toFixed(4);

        case "CP":
        case "ICP":
            var proceedsCp = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsCp / nominal) * 100;
            return result.toFixed(2.5);

        case "BA":
        case "AB-i":
            var proceedsBa = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsBa / nominal) * 100;
            return result.toFixed(4);

        case "BNMN":
        case "BNMN-i":
        case "MTB":
        case "MTIB":
            var proceedsOthers = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsOthers / nominal) * 100;
            return result.toFixed(4);

        case "NIDL":
            var proceedsNidl = _treasury.inflow_proceeds(productType, nominal, rate, tenor);
            result = (proceedsNidl / nominal) * 100;
            return result.toFixed(2.5);

        default:
            return result;
        }
    }
    
    _treasury.dsNotes = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _treasury.api.loadNotes
            }),
            paginate: true,
            pageSize: 20
        };
    };

    _treasury.dsAssetType = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _treasury.api.loadAssetType
            }),
            paginate: true,
            pageSize: 20
        };
    };

    _treasury.dsProductType = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "id",
                loadUrl: _treasury.api.loadProductType
            }),
            paginate: true,
            pageSize: 20
        };
    };

    _treasury.dsIssuer = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "reference",
                loadUrl: _treasury.api.loadIssuer
            }),
            paginate: true,
            pageSize: 20
        };
    };

    _treasury.dsBankCounterParty = function () {
        return {
            store: DevExpress.data.AspNet.createStore({
                key: "reference",
                loadUrl: _treasury.api.loadCounterParty
            }),
            paginate: true,
            pageSize: 20
        };
    };


    return _treasury;
}());