<template>
	<div>
		<h2>Accounts</h2>
		<div v-for="broker in brokers" :key="broker.id">
			<h3>{{ broker.displayName }}</h3>
			<div class="row pl-3">
				<div v-for="account in broker.accounts" :key="account.id">
					<account-card :account="account" class="mr-2" />
				</div>
				<add-account-card :broker-id="broker.id" />
			</div>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import AccountCard from '@/component/config/accountCard.vue';
import AddAccountCard from '@/component/config/addAccountCard.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	name: 'AccountsView',
	components: {
		AccountCard,
		AddAccountCard,
	},
})
export default class AccountsView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	get brokers() {
		return this.activeState.brokers;
	}
}
</script>
