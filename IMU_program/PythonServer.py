import serial.tools.list_ports
import serial
import socket


ports = list(serial.tools.list_ports.comports())
arduino_port = next((port for port in ports if "Arduino" in port.description), None)

arduino = serial.Serial(arduino_port[0], 9600)

PORT = 4242
HOST = 'localhost'

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(5)

while True:
    connection, adress = server_socket.accept()
    print("Client connected")
    while True:
        try:
            incoming = arduino.readline()
            connection.send(incoming)
        except:
            print("Client disconnected")
            break
