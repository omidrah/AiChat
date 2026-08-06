import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActiveDirectorySettings } from './active-directory-settings';

describe('ActiveDirectorySettings', () => {
  let component: ActiveDirectorySettings;
  let fixture: ComponentFixture<ActiveDirectorySettings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActiveDirectorySettings],
    }).compileComponents();

    fixture = TestBed.createComponent(ActiveDirectorySettings);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
