import { LogoElement } from './logo';

describe('logo', () => {
  it('should work', () => {
    const logo = new LogoElement();
    expect(logo.src).toEqual('');
  });
});
