import {Injectable} from '@angular/core';
import {Actions, createEffect, ofType} from '@ngrx/effects';
import {of} from 'rxjs';
import {map, catchError, exhaustMap, tap, delay} from 'rxjs/operators';
import {AuthService} from '@app/services/auth.service';
import {
  confirmEmail,
  confirmEmailFailure,
  confirmEmailSuccess,
  externalLogin,
  login,
  loginFailure,
  loginSuccess,
  logout,
  logoutFailure,
  logoutSuccess,
  register,
  registerFailure,
  registerSuccess
} from '../auth.actions';
import {CacheService} from '@app/services/cache.service';
import {Router} from '@angular/router';

@Injectable()
export class AuthEffects {
  externalLogin$ = createEffect(() =>
    this.actions$.pipe(
      ofType(externalLogin),
      exhaustMap(action =>
        // FB or other external Login
        this.authService.externalLogin(action.provider).pipe(
          map(user => {
            console.log('effects', user);
            this.cacheService.setUser(user);
            this.cacheService.setToken(user.token);
            return loginSuccess({user});
          }),
          catchError(loginError => {
            return of(loginFailure({loginError: {message: loginError.message, status: loginError.status }}));
          })
        )
      )
    )
  );

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(login),
      exhaustMap(action =>
        this.authService.login(action.userName, action.password).pipe(
          map(user => {
            this.cacheService.setUser(user);
            this.cacheService.setToken(user.token);
            return loginSuccess({user});
          }),
          catchError((loginError) => {
            console.error('createEffect :: login failed', loginError);
            return of(loginFailure({loginError: {message: loginError.message, status: loginError.status }}));
          })
        )
      )
    )
  );

  loginSuccess$ = createEffect(() =>
      this.actions$.pipe(
        ofType(loginSuccess),
        tap(() => this.router.navigate(['/dashboard'])
        )
      ),
    {dispatch: false}
  );

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(logout),
      exhaustMap(action => {
          return this.authService.logout().pipe(
            map(result => {
              return logoutSuccess();
            }),
            catchError(error => {
              return of(logoutFailure());
            })
          );
        }
      )
    )
  );

  logoutSuccess$ = createEffect(() =>
      this.actions$.pipe(
        ofType(logoutSuccess, logoutFailure),
        tap(() => {
          this.cacheService.setUser(null);
          this.cacheService.setToken('');
        }),
        tap(() => {
          console.log('logout success, redirecting to login page');
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 1000);
        })
      ),
    {dispatch: false}
  );

  register$ = createEffect(() =>
    this.actions$.pipe(
      ofType(register),
      exhaustMap(action =>
        this.authService.register(action.userName, action.email, action.password, action.confirmPassword).pipe(
          map(user => {
            console.log('register effects', user);
            return registerSuccess({user});
          }),
          // TODO it should update AuthState.registerError
          catchError(error => of(registerFailure({registerError: error})))
        )
      )
    )
  );

  registerSuccess$ = createEffect(() =>
      this.actions$.pipe(
        ofType(registerSuccess),
        tap(() => this.router.navigate(['/login']))
      ),
    {dispatch: false}
  );

  confirmEmail$ = createEffect(() =>
    this.actions$.pipe(
      ofType(confirmEmail),
      exhaustMap(action =>
        this.authService.confirmEmail(action.userId, action.code).pipe(
          map(result => {
            console.log('effects', result);
            if (result.succeeded === true) {
              return confirmEmailSuccess();
            }
            return confirmEmailFailure({confirmEmailError: result.errors});
          }),
          // TODO it should update AuthState.confirmEmailError
          catchError(error => of(confirmEmailFailure({confirmEmailError: error})))
        )
      )
    )
  );

  constructor(
    private actions$: Actions,
    private router: Router,
    private authService: AuthService,
    private cacheService: CacheService,
  ) {
  }
}
