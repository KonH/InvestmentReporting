<template>
	<h1>Home</h1>
	<currencyView />
	<brokerView />
	<button :onclick="resetOps" class="btn btn-danger mt-2">Reset (ops)</button>
	<button :onclick="resetAll" class="btn btn-danger mt-2 ml-2">Reset (all)</button>
	<button :onclick="logout" class="btn btn-secondary mt-2 ml-2">Logout</button>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import CurrencyView from '@/view/currencyView.vue';
import BrokerView from '@/view/brokerView.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	name: 'Home',
	components: {
		CurrencyView,
		BrokerView,
	},
})
export default class Home extends Vue {
	@State('activeState')
	activeState!: StateDto;

	get currencies() {
		return this.activeState.currencies;
	}

	get brokers() {
		return this.activeState.brokers;
	}

	async resetOps() {
		await Backend.state().operation.operationDelete();
		await router.go(0);
	}

	async resetAll() {
		await Backend.state().state.stateDelete();
		await router.go(0);
	}

	async logout() {
		await Backend.auth().logout.logoutCreate();
		await router.go(0);
	}
}
</script>
