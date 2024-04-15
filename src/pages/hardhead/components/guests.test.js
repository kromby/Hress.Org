import React from 'react';
import { act } from 'react-dom/test-utils';
const axios = require('axios');
import Guests from './guests';

jest.mock('axios');

describe('Guests', () => {
  let container = null;

  beforeEach(() => {
    // Set up a DOM element as a render target
    container = document.createElement('div');
    document.body.appendChild(container);
  });

  afterEach(() => {
    // Clean up on exiting
    ReactDOM.unmountComponentAtNode(container);
    container.remove();
  });

  it('renders without crashing', async () => {
    const guests = [
      { id: 1, username: 'guest1', profilePhoto: { href: 'photo1.jpg' } },
      { id: 2, username: 'guest2', profilePhoto: { href: 'photo2.jpg' } },
    ];

    axios.get.mockResolvedValueOnce({ data: guests });

    // Render the component
    await act(async () => {
      render(<Guests hardheadID={123} />, container);
    });

    // Check if the guests are rendered correctly
    expect(container.querySelectorAll('.col-2').length).toBe(guests.length);

    // Clean up
    axios.get.mockClear();
  });

  it('handles error when retrieving guests', async () => {
    const error = new Error('Failed to retrieve guests');
    axios.get.mockRejectedValueOnce(error);

    // Render the component
    await act(async () => {
      ReactDOM.render(<Guests hardheadID={123} />, container);
    });

    // Check if the error is displayed
    expect(container.textContent).toContain('Error retrieving guests');

    // Clean up
    axios.get.mockClear();
  });
});