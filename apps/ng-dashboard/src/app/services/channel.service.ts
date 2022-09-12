import {Injectable, Inject} from '@angular/core';
import {Observable} from 'rxjs/Observable';
import {Subject} from 'rxjs/Subject';
import {HubConnection, HubConnectionBuilder, LogLevel} from '@aspnet/signalr';
import {environment} from '../../environments/environment';

/**
 * When SignalR runs it will add functions to the global $ variable
 * that you use to create connections to the hub. However, in this
 * class we won't want to depend on any global variables, so this
 * class provides an abstraction away from using $ directly in here.
 */
export class SignalrWindow extends Window {
  $: any;
}

export enum ConnectionState {
  Connecting = 1,
  Connected = 2,
  Reconnecting = 3,
  Disconnected = 4
}

export class ChannelConfig {
  url: string;
  hubName: string;
  channel: string;
}

export class ChannelEvent {
  Name: string;
  ChannelName: string;
  Timestamp: Date;
  Data: any;
  Json: string;

  constructor() {
    this.Timestamp = new Date();
  }
}

class ChannelSubject {
  channel: string;
  subject: Subject<ChannelEvent>;
}

/**
 * ChannelService is a wrapper around the functionality that SignalR
 * provides to expose the ideas of channels and events. With this service
 * you can subscribe to specific channels (or groups in signalr speak) and
 * use observables to react to specific events sent out on those channels.
 */
@Injectable()
export class ChannelService {

  /**
   * starting$ is an observable available to know if the signalr
   * connection is ready or not. On a successful connection this
   * stream will emit a value.
   */
  starting$: Observable<any>;

  /**
   * connectionState$ provides the current state of the underlying
   * connection as an observable stream.
   */
  connectionState$: Observable<ConnectionState>;

  /**
   * error$ provides a stream of any error messages that occur on the
   * SignalR connection
   */
  error$: Observable<string>;

  // These are used to feed the public observables
  //
  private connectionStateSubject = new Subject<ConnectionState>();
  private startingSubject = new Subject<any>();
  private errorSubject = new Subject<any>();

  // These are used to track the internal SignalR state
  //
  private _hubConnection: any;
  private subjects = new Array<ChannelSubject>();

  constructor(
    @Inject(SignalrWindow) private window: SignalrWindow,
  ) {

    // this._hubConnection = new HubConnectionBuilder()
    //   .withUrl(`${environment.hubBaseUrl}eventing?t=${jwtToken}`)
    //   .configureLogging(LogLevel.Trace)
    //   .build();
    //
    // this._hubConnection.on('Send', (data: any) => {
    //   const received = `Received: ${JSON.stringify(data)}`;
    //   console.log(received);
    // });

  }

  /**
   * Start the SignalR connection. The starting$ stream will emit an
   * event if the connection is established, otherwise it will emit an
   * error.
   */
  start(): void {
    // Now we only want the connection started once, so we have a special
    //  starting$ observable that clients can subscribe to know know if
    //  if the startup sequence is done.
    //
    // If we just mapped the start() promise to an observable, then any time
    //  a client subscribed to it the start sequence would be triggered
    //  again since it's a cold observable.
    //
    this._hubConnection.start()
      .done(() => {
        this.startingSubject.next();
      })
      .fail((error: any) => {
        this.startingSubject.error(error);
      });
  }

  /**
   * Get an observable that will contain the data associated with a specific
   * channel
   * */
  sub(channel: string): Observable<ChannelEvent> {

    // Try to find an observable that we already created for the requested
    //  channel
    //
    let channelSub = this.subjects.find((x: ChannelSubject) => {
      return x.channel === channel;
    }) as ChannelSubject;

    // If we already have one for this event, then just return it
    //
    if (channelSub !== undefined) {
      console.log(`Found existing observable for ${channel} channel`);
      return channelSub.subject.asObservable();
    }

    //
    // If we're here then we don't already have the observable to provide the
    //  caller, so we need to call the server method to join the channel
    //  and then create an observable that the caller can use to received
    //  messages.
    //

    // Now we just create our internal object so we can track this subject
    //  in case someone else wants it too
    //
    channelSub = new ChannelSubject();
    channelSub.channel = channel;
    channelSub.subject = new Subject<ChannelEvent>();
    this.subjects.push(channelSub);

    // Now SignalR is asynchronous, so we need to ensure the connection is
    //  established before we call any server methods. So we'll subscribe to
    //  the starting$ stream since that won't emit a value until the connection
    //  is ready
    //
    this.starting$.subscribe(() => {
        this._hubConnection.invoke('Subscribe', channel)
          .done(() => {
            console.log(`Successfully subscribed to ${channel} channel`);
          })
          .fail((error: any) => {
            channelSub.subject.error(error);
          });
      },
      (error: any) => {
        channelSub.subject.error(error);
      });

    return channelSub.subject.asObservable();
  }

  // Not quite sure how to handle this (if at all) since there could be
  //  more than 1 caller subscribed to an observable we created
  //
  // unsubscribe(channel: string): Rx.Observable<any> {
  //     this.observables = this.observables.filter((x: ChannelObservable) => {
  //         return x.channel === channel;
  //     });
  // }

  /** publish provides a way for callers to emit events on any channel. In a
   * production app the server would ensure that only authorized clients can
   * actually emit the message, but here we're not concerned about that.
   */
  publish(ev: ChannelEvent): void {
    this._hubConnection.invoke('Send', ev);
  }

}
