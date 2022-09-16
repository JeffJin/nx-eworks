import {Component, NgZone, OnInit, ViewEncapsulation} from '@angular/core';
import {ChannelService} from '../../services/channel.service';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {HubConnection, HubConnectionBuilder, LogLevel} from '@aspnet/signalr';
import {Utils} from '../../services/utils';
import {VideoDto} from '../../models/dtos';
import {DomSanitizer} from '@angular/platform-browser';
import {environment} from '../../../environments/environment';
import {CacheService} from '../../services/cache.service';

@Component({
  selector: 'app-list-videos',
  templateUrl: './list-videos.component.html',
  styleUrls: ['./list-videos.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ListVideosComponent implements OnInit {
  videos: VideoDto[] = [];
  private _hubConnection: HubConnection;
  private _socketConnection: WebSocket;
  messages: string[] = [];
  currentVideo: VideoDto = null;

  constructor(private channelService: ChannelService, private sanitizer: DomSanitizer,
              private cacheService: CacheService,
              private zone: NgZone, private utils: Utils, private httpClient: HttpClient) {

  }

  ngOnInit() {
    this.loadVideos();
    this.startSocketConnection();
  }

  getUrl(url: string) {
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }

  startHubConnection() {
    // const transportType = TransportType[this.utils.getParameterByName('transport')] || TransportType.WebSockets;

    const jwtToken = this.cacheService.getToken();
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}eventing?t=${jwtToken}`)
      .configureLogging(LogLevel.Critical)
      .build();

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

  clockMessaging() {
    this.httpClient.get('http://127.0.0.1:5000/clockHub')
      .subscribe((result: any) => {
          console.log(result);
        }
      );
  }


  loadVideos() {
    this.httpClient.get('http://127.0.0.1:5000/api/video')
      .subscribe((results: any) => {
          this.videos = results.map(r => {
            return {
              id: r.sourceId,
              url: r.progressiveUrl,
              category: r.category.name,
              type: r.sourceType,
              createdBy: r.email,
              title: r.title,
              description: r.description
            };
          });
        }
      );
  }

  deleteAsset(assetName: string) {
    this.httpClient.delete('http://127.0.0.1:5000/api/video/' + assetName)
      .subscribe((result: any) => {
          console.log(result);
        },
        (error) => {
          console.error(error);
        }
      );
  }

  publishAsset(assetName: string) {
    const headers = new HttpHeaders();
    const options = {
      headers: headers
    };

    this.httpClient.post('http://127.0.0.1:5000/api/video/publish/' + assetName,
      null, options)
      .subscribe(
        data => console.log('success'),
        error => console.log(error)
      );
  }

}
