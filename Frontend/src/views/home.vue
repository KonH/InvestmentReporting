<template>
	<h1>Home</h1>
	<div v-for="broker in brokers" :key="broker.displayName">
		<broker :broker="broker" />
	</div>
	<button :onclick="logout" class="btn btn-secondary">Logout</button>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import Broker from '@/component/broker.vue';
import { StateDto } from '@/api/state';
import { Action, State } from 'vuex-class';

@Options({
	components: {
		Broker,
	},
})
export default class Home extends Vue {
	@State('activeState') activeState!: StateDto;
	@Action('fetchActiveState') fetchActiveState!: () => void;

	get brokers() {
		return this.activeState.brokers;
	}

	async created() {
		this.fetchActiveState();
	}

	async logout() {
		await Backend.auth().logout.logoutCreate();
		await router.go(0);
	}
}
</script>
