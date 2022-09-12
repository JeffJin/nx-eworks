import {Component, ViewEncapsulation} from '@angular/core';
import {AuthService} from './services/auth.service';
import {DomSanitizer} from '@angular/platform-browser';
import {MatIconRegistry} from '@angular/material/icon';
import {Store} from '@ngrx/store';
import { CalendarOptions, defineFullCalendarElement } from '@fullcalendar/web-component';
import dayGridPlugin from '@fullcalendar/daygrid';

// make the <full-calendar> element globally available by calling this function at the top-level
defineFullCalendarElement();

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AppComponent {

  constructor(private authService: AuthService,
              private matIconRegistry: MatIconRegistry,
              private domSanitizer: DomSanitizer,
              private store: Store
  ){
    matIconRegistry.addSvgIcon(
      'notification',
      domSanitizer.bypassSecurityTrustResourceUrl('../assets/icons/noun_notification_1439225_FFFFFF.svg')
    ).addSvgIcon(
      'notification-marked',
      domSanitizer.bypassSecurityTrustResourceUrl('../assets/icons/noun_notification_1439226_FFFFFF.svg')
    );
  }
}
