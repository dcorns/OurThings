App = Ember.Application.create();
App.Router.map(function () {
    this.resource('adduser');
    this.resource('deluser');
    this.resource('edituser');
});