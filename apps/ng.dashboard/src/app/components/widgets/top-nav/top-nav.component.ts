import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {UiService} from '@app/services/ui.service';
import {AuthService} from '@app/services/auth.service';
import {Router} from '@angular/router';
import {Store} from '@ngrx/store';
import {UserDto} from '@app/models/dtos';
import {Observable} from 'rxjs/Observable';
import {selectUser} from '@app/states/auth/auth.reducers';
import {logout} from '@app/states/auth/auth.actions';

@Component({
  selector: 'app-top-nav',
  templateUrl: './top-nav.component.html',
  styleUrls: ['./top-nav.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class TopNavComponent implements OnInit {
  user$: Observable<UserDto>;
  isBusy = false;
  sideBarToggled = false;
  searchKey = '';

  constructor(private uiService: UiService, private router: Router,
              private authService: AuthService, private store: Store) {
    this.user$ = this.store.select(selectUser);
  }

  ngOnInit(): void {

  }

  toggleSideMenu(): void  {
    setTimeout(() => {
      this.sideBarToggled = !this.sideBarToggled;
    }, 300);
    this.uiService.toggleSideMenu();
  }

  toggleNotification(): void  {
    this.uiService.toggleNotification();
  }

  logoutUser(): void {
    this.store.dispatch(logout());
  }

  search(searchKey: string): void {

  }
}
