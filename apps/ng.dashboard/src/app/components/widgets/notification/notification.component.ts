import {Component, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {MatSidenav} from '@angular/material/sidenav';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class NotificationComponent implements OnInit {
  @ViewChild('rightPanel') rightPanel: MatSidenav;

  constructor() { }

  ngOnInit(): void {
  }

  toggle(): void{
    this.rightPanel.toggle();
  }

}
