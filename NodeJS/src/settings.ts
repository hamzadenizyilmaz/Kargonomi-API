import appSettings from '../appsettings.json' with { type: 'json' };

export const configuredBaseUrl: string = appSettings.Kargonomi.BaseUrl;
