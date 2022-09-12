import {Component, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit} from '@angular/core';
import {UntypedFormControl, UntypedFormGroup, Validators} from '@angular/forms';
import {AuthService} from '@app/services/auth.service';
import {Router} from '@angular/router';
import {Store} from '@ngrx/store';
import {UserService} from '@app/services/user.service';
import {Observable} from 'rxjs/Observable';
import {CustomValidators} from '@app/handlers/custom-validators';
import {selectRegisterError} from '@app/states/auth/auth.reducers';
import {register} from '@app/states/auth/auth.actions';

@Component({
  selector: 'app-register',
  styleUrls: ['./register.component.scss'],
  templateUrl: './register.component.html'
})

export class RegisterComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('card') cardElm: ElementRef;

  registerForm: UntypedFormGroup;
  registerError: string;
  registerError$: Observable<any>;
  userNameControl: UntypedFormControl;
  emailControl: UntypedFormControl;
  agreeTnCControl: UntypedFormControl;
  passwordControl: UntypedFormControl;
  confirmPasswordControl: UntypedFormControl;

  constructor(private elementRef: ElementRef,
              private authService: AuthService,
              private userService: UserService,
              private router: Router,
              private store: Store) {
    this.registerError$ = this.store.select(selectRegisterError);
  }

  ngOnInit(): void {
    this.initializeForm();
    setTimeout(() => {
      // after 500 ms we add the class animated to the login/register card
      this.cardElm.nativeElement.classList.remove('card-hidden');
    }, 500);
  }

  ngAfterViewInit(): void {
  }

  ngOnDestroy(): void {
  }

  initializeForm(): void {
    this.userNameControl = new UntypedFormControl('',
      [
        Validators.minLength(5),
        Validators.required,
        Validators.maxLength(32),
      ],
      CustomValidators.createUserNameValidator(this.userService)
    );

    this.emailControl = new UntypedFormControl('',
      [
        Validators.required,
        Validators.email
      ],
      CustomValidators.createEmailValidator(this.userService)
    );

    this.passwordControl = new UntypedFormControl('',
      [
        Validators.required,
        Validators.minLength(6),
      ]
    );

    this.confirmPasswordControl = new UntypedFormControl('',
      [
        Validators.required,
        Validators.minLength(6),
        CustomValidators.validatePasswordsMatch(this.passwordControl)
      ]
    );

    this.agreeTnCControl = new UntypedFormControl(false,
      Validators.requiredTrue
    );

    this.registerForm = new UntypedFormGroup({
      userName: this.userNameControl,
      email: this.emailControl,
      password: this.passwordControl,
      confirmPassword: this.confirmPasswordControl,
      agreeTnC: this.agreeTnCControl,
    });
  }

  handleRegister(evt: Event): void {
    evt.stopPropagation();
    if (!this.registerForm.valid) {
      this.registerError = 'The information entered is not valid!';
      return;
    }
    const userName = this.registerForm.value.userName;
    const email = this.registerForm.value.email;
    const password = this.registerForm.value.password;
    const confirmPassword = this.registerForm.value.confirmPassword;
    this.store.dispatch(register({userName, email, password, confirmPassword}));
  }

  facebookSignup(): void {

  }

  googleSignup(): void {

  }

  twitterSignup(): void {

  }
}
