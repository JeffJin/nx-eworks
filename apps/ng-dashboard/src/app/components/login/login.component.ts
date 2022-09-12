import {ChangeDetectionStrategy, Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {AuthService} from '@app/services/auth.service';
import {UntypedFormBuilder, UntypedFormGroup, Validators} from '@angular/forms';
import {Router} from '@angular/router';
import {Store} from '@ngrx/store';
import {Observable} from 'rxjs/Observable';
import {UserDto} from '@app/models/dtos';
import {externalLogin, login, resetLoginError} from '@app/states/auth/auth.actions';
import {selectLoginError, selectUser} from '@app/states/auth/auth.reducers';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class  LoginComponent implements OnInit {
  @ViewChild('card') cardElm: ElementRef;

  loginForm: UntypedFormGroup;
  user$: Observable<UserDto>;
  loginError$: Observable<any>;
  private sidebarVisible: boolean;
  private nativeElement: Node;

  constructor(private fb: UntypedFormBuilder, private elementRef: ElementRef,
              private authService: AuthService,
              private router: Router, private store: Store) {
    this.user$ = this.store.select(selectUser);
    this.loginError$ = this.store.select(selectLoginError);
    this.nativeElement = elementRef.nativeElement;
    this.sidebarVisible = false;
  }

  ngOnInit(): void {
    this.initializeForm();
    setTimeout(() => {
      // after 1000 ms we add the class animated to the login/register card
      this.cardElm.nativeElement.classList.remove('card-hidden');
    }, 1000);
  }

  resetError(): void {
    this.store.dispatch(resetLoginError());
  }

  initializeForm(): void {
    this.loginForm = this.fb.group({
      email: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  loginToSite(): void {
    const userName = this.loginForm.value.email;
    const password = this.loginForm.value.password;
    this.store.dispatch(login({userName, password}));
  }

  goToSignup(): Promise<boolean> {
    return this.router.navigateByUrl('signup');
  }

  handleExternalLogin(provider: string): void {
    this.store.dispatch(externalLogin({provider}));
  }

  passwordChanged($event: Event): void {
    this.resetError();
  }

  emailChanged($event: Event): void {
    this.resetError();
  }
}
