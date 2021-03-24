import { Api as Auth } from '@/api/auth';
import { Api as Invite } from '@/api/invite';
import { Api as State } from '@/api/state';

export default class Backend {
	static auth() {
		return new Auth({ baseUrl: '/api/auth/v1' });
	}

	static invite() {
		return new Invite({ baseUrl: '/api/invite/v1' });
	}

	static state() {
		return new State({ baseUrl: '/api/state/v1' });
	}

	static async tryFetch<T>(promise: Promise<T>) {
		try {
			return await promise;
		} catch (e) {
			console.log(e);
			return undefined;
		}
	}
}
