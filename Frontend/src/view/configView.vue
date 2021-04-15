<template>
	<button :onclick="sync" class="btn btn-primary">Sync</button>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action } from 'vuex-class';

export default class ConfigView extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	async sync() {
		await Backend.tryFetch(Backend.market().sync.syncCreate());
		this.fetchState();
		await router.push('/');
	}
}
</script>
