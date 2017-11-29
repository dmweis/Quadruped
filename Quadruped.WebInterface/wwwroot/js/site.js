function createJoystick(data) {
    const vm = new Vue({
        el: data.elementId,
        data: {
            angle: 0,
            distance: 0,
            color: data.color,
            name: data.name,
            touchDown: false
        },
        mounted: function () {
            const context = this;
            nipplejs.create({ zone: this.$el, color: this.color }).on('added',
                function (evt, nipple) {
                    nipple.on('move',
                        function (evt, arg) {
                            context.angle = arg.angle.radian;
                            context.distance = arg.distance;
                        });
                    nipple.on('start',
                        function () {
                            context.angle = 0;
                            context.distance = 0;
                            context.touchDown = true;
                            if (data.startCallback) {
                                data.startCallback();
                            }
                        });
                    nipple.on('end',
                        function () {
                            context.angle = 0;
                            context.distance = 0;
                            context.touchDown = false;
                            if (data.endCallback) {
                                data.endCallback();
                            }
                        });
                });
        },
        watch: {
            touchDown: function() {
                data.socket.send(JSON.stringify({
                    joystickType: this.name,
                    MessageType: this.touchDown ? 'start' : 'stop'
                }));
            }
        }
    });
    vm.$watch(function () {
        return this.angle + this.distance;
    },
        function (newValue, oldValue) {
            data.socket.send(JSON.stringify({
                joystickType: this.name,
                angle: this.angle,
                MessageType: 'movement',
                distance: this.distance
            }));
        });
    return vm;
}

function createServerConnection() {
    const uri = `ws://${document.domain}:${location.port}/ws`;
    var serverSocket = new WebSocket(uri);
    window.onbeforeunload = function () {
        serverSocket.onclose = function () { };
        serverSocket.close();
    };
    return serverSocket;
}