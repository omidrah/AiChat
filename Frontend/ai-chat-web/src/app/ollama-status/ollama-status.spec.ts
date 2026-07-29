import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OllamaStatus } from './ollama-status';

describe('OllamaStatus', () => {
  let component: OllamaStatus;
  let fixture: ComponentFixture<OllamaStatus>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OllamaStatus],
    }).compileComponents();

    fixture = TestBed.createComponent(OllamaStatus);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
