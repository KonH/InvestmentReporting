import { Api as Auth } from '@/api/auth';
import { Api as Invite } from '@/api/invite';
import { Api as State } from '@/api/state';
import { Api as Import } from '@/api/import';
import { Api as Market } from '@/api/market';
import { Api as Meta } from '@/api/meta';

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

	static import() {
		return new Import({ baseUrl: '/api/import/v1' });
	}

	static market() {
		return new Market({ baseUrl: '/api/market/v1' });
	}

	static meta() {
		return new Meta({ baseUrl: '/api/meta/v1' });
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
