import {Action, createFeatureSelector, createReducer, createSelector, on} from '@ngrx/store';
import {
  confirmEmailFailure,
  confirmEmailSuccess,
  loginFailure,
  loginSuccess,
  logoutFailure,
  logoutSuccess,
  registerFailure, resetLoginError
} from './auth.actions';
import {AuthState} from './auth.state';

export const initialState: AuthState = {
  user: {
    userName: '',
    email: '',
    phoneNumber: '',
  },
  token: '',
  loginError: null,
  registerError: null,
  confirmEmailSuccess: null,
  confirmEmailError: null
};

const getAuthFeatureState = createFeatureSelector<AuthState>('auth');

export const selectUser = createSelector(
  getAuthFeatureState,
  state => state.user
);

export const selectLoginError = createSelector(
  getAuthFeatureState,
  state => state.loginError
);

export const selectConfirmEmailError = createSelector(
  getAuthFeatureState,
  state => state.confirmEmailError
);

export const selectConfirmEmailSuccess = createSelector(
  getAuthFeatureState,
  state => state.confirmEmailSuccess
);

export const selectRegisterError = createSelector(
  getAuthFeatureState,
  state => state.registerError
);

export const authReducer = createReducer(
  initialState,
  on(resetLoginError, (state) => {
    console.log('resetLoginError');
    return {...state, loginError: null};
  }),
  on(loginSuccess, (state, { user }) => {
    console.log('authReducerInternal::loginSuccess', user);
    return {...state, user};
  }),
  on(loginFailure, (state, { loginError }) => {
    console.log('authReducerInternal::loginFailure', loginError);
    return {...state, loginError};
  }),
  on(logoutSuccess, (state) => {
    console.log('authReducerInternal::logoutSuccess');
    return undefined;
  }),
  on(logoutFailure, (state) => {
    console.log('authReducerInternal::logoutFailure');
    return undefined;
  }),
  on(registerFailure, (state, { registerError }) => {
    console.log('authReducerInternal::registerFailure', registerError);
    return {...state, registerError};
  }),
  on(confirmEmailSuccess, (state) => {
    console.log('authReducerInternal::confirmEmailSuccess');
    return {...state, confirmEmailSuccess: true};
  }),
  on(confirmEmailFailure, (state, { confirmEmailError }) => {
    console.log('authReducerInternal::confirmEmailError', confirmEmailError);
    return {...state, confirmEmailError};
  }),
);

