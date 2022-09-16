import {createAction, props} from '@ngrx/store';
import {UserDto} from '@app/models/dtos';

export const externalLogin = createAction(
  '[authService.externalLogin] EXTERNAL LOGIN',
  props<{ provider: string }>()
);

export const login = createAction(
  '[authService.login] LOGIN',
  props<{ userName: string, password: string }>()
);

export const resetLoginError = createAction(
  '[resetLoginError] LOGIN ERROR'
);

export const loginSuccess = createAction(
  '[authService.loginSuccess] LOGIN SUCCESS',
  props<{ user: UserDto }>()
);

export const loginFailure = createAction(
  '[authService.login] LOGIN FAILURE',
  props<{ loginError: any }>()
);

export const logout = createAction(
  '[authService.logout] LOGOUT'
);

export const logoutSuccess = createAction(
  '[authService.logoutSuccess] LOGOUT SUCCESS'
);

export const logoutFailure = createAction(
  '[authService.logoutFailure] LOGOUT FAILURE'
);

export const confirmEmail = createAction(
  '[authService.confirmEmail] CONFIRM-EMAIL',
  props<{ code: string, userId: string }>()
);
export const confirmEmailSuccess = createAction(
  '[authService.confirmEmail] confirmEmailSuccess'
);

export const confirmEmailFailure = createAction(
  '[authService.confirmEmail] confirmEmailFailure',
  props<{ confirmEmailError: null }>()
);

export const register = createAction(
  '[authService.register] REGISTER',
  props<{ userName: string, email: string, password: string, confirmPassword: string }>()
);

export const registerSuccess = createAction(
  '[authService.registerSuccess] REGISTER SUCCESS',
  props<{ user: UserDto }>()
);

export const registerFailure = createAction(
  '[authService.register] REGISTER FAILURE',
  props<{ registerError: string }>()
);
