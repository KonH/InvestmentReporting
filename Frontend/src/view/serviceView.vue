<template>
	<div>
		<h2>Service</h2>
		<button :onclick="sync" class="btn btn-primary">Sync</button>
		<button :onclick="resetOps" class="btn btn-danger ml-2">Reset (operations)</button>
		<button :onclick="resetAll" class="btn btn-danger ml-2">Reset (full)</button>
		<button :onclick="logout" class="btn btn-secondary ml-2">Logout</button>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action } from 'vuex-class';

@Options({
	name: 'ServiceView',
})
export default class ConfigView extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	async sync() {
		await Backend.tryFetch(Backend.market().sync.syncCreate());
		this.fetchState();
		await router.push('/');
	}

	async resetOps() {
		await Backend.state().operation.operationDelete();
		await router.push('/config');
	}

	async resetAll() {
		await Backend.state().state.stateDelete();
		await router.push('/config');
	}

	async logout() {
		await Backend.auth().logout.logoutCreate();
		await router.push('/');
	}
}
</script>
