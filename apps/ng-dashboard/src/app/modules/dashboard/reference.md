  startHubConnection() {
    const transportType = TransportType[this.utils.getParameterByName('transport')] || TransportType.WebSockets;

    this._hubConnection = new HubConnection('http://127.0.0.1:5000/eventing',
      { transport: transportType, logging: new ConsoleLogger(LogLevel.Information) });

    this._hubConnection.on('Send', (data: any) => {
      const received = `Received: ${(data)}`;
      console.log(received);
      this.messages.push(received);
    });

    this._hubConnection.start()
      .then(() => {
        console.log('Hub connection started');
      })
      .catch(err => {
        console.log('Error while establishing connection');
      });
  }

  stopHubConnection() {
    this._hubConnection.stop();
    console.log('Hub connection stopped');
  }

  startSocketConnection() {
    this._socketConnection = new WebSocket(`ws://localhost:5000/messaging`);

    this._socketConnection.onopen = function () {
      console.log('Web socket opened to ', 'ws://localhost:5000/messaging');
    };

    this._socketConnection.onmessage = function (event) {
      console.log('Web socket received', event.data);
    };

    this._socketConnection.onclose = function (event) {
      console.log('Web socket closed!');
    };
  }

  stopSocketConnection() {
    if (this._socketConnection) {
      this._socketConnection.close();
      console.log('socket connection stopped');
    }
  }

  public joinGroup(groupName): void {
    this._hubConnection.invoke('JoinGroup', groupName);
  }

  public leaveGroup(groupName): void {
    this._hubConnection.invoke('LeaveGroup', groupName);
  }

  public sendPublicMessage(msg): void {
    this._hubConnection.invoke('SendToPublic', msg);
    this.publicMessages.push(msg);
  }

  public sendGroupMessage(msg, group): void {
    this._hubConnection.invoke('SendToGroup', msg, group);
    this.groupMessages.push(msg);
  }

  public sendUserMessage(msg): void {
    this._hubConnection.invoke('SendToUser', msg);
    this.userMessages.push(msg);
  }
