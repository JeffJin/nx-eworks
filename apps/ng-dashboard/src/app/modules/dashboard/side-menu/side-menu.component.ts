import {Component, OnInit, ViewChild} from '@angular/core';
import {MatSidenav} from '@angular/material/sidenav';

@Component({
  selector: 'app-left-menu',
  templateUrl: './side-menu.component.html',
  styleUrls: ['./side-menu.component.scss']
})
export class SideMenuComponent implements OnInit {
  @ViewChild('leftNav') leftNav: MatSidenav;
  profileMenuOpen: boolean;
  deviceMenuOpen: boolean;

  constructor() { }

  ngOnInit(): void {
    this.profileMenuOpen = false;
    this.deviceMenuOpen = false;
  }

  toggle(): any {
    return this.leftNav.toggle();
  }

  toggleProfileMenu(): void {
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  toggleDeviceMenuOpen(): void {
    this.deviceMenuOpen = !this.deviceMenuOpen;
  }
}
