

$.post("/DRCCustomer/GetCustomers", function (data) { });
function CustomerInfo(AcctID, Name, MainPhone, Fax, ADRStreet, ADRCity, ADRState, ADRZip, ADRCountry, OpeningBalDate, OpeningBal, Terms, WebSite) {
    this.AcctID = AcctID;
    this.name = Name;
    this.MainPhone = MainPhone;
    this.Fax = Fax;
    this.ADRStreet = ADRStreet;
    this.ADRState = ADRState;
    this.ADRZip = ADRZip;
    this.ADRCountry = ADRCountry;
    this.OpeningBalDate = OpeningBalDate;
    this.OpeningBal = OpeningBal;
    this.Terms = Terms;
    this.Website = WebSite;
}
