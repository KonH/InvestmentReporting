<template>
	<div>
		<h2>Context</h2>
		<div class="form-group">
			<label>
				Broker:
				<select ref="broker" class="form-control" @change="onBrokerChange">
					<option v-for="broker in brokers" :key="broker.id" :value="broker.id">
						{{ broker.displayName }}
					</option>
				</select>
			</label>
		</div>
		<div class="form-group">
			<label>
				Account:
				<select ref="account" class="form-control" @change="onAccountChange">
					<option v-for="account in accounts" :key="account.id" :value="account.id">
						{{ account.displayName }}
					</option>
				</select>
			</label>
		</div>

		<h2>Assets</h2>
		<router-link v-if="brokerId" :to="`/custom/broker/${brokerId}/asset/buy`" class="btn btn-primary">Buy</router-link>
		<router-link v-if="brokerId" :to="`/custom/broker/${brokerId}/asset/sell`" class="btn btn-danger ml-2">Sell</router-link>

		<h2>Operations</h2>
		<router-link v-if="brokerId && accountId" :to="`/custom/broker/${brokerId}/account/${accountId}/income`" class="btn btn-primary">Income</router-link>
		<router-link v-if="brokerId && accountId" :to="`/custom/broker/${brokerId}/account/${accountId}/expense`" class="btn btn-danger ml-2">Expense</router-link>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import { AccountDto, BrokerDto, StateDto } from '@/api/state';
import { Ref } from 'vue-property-decorator';

@Options({
	name: 'CustomView',
})
export default class ConfigView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Ref('broker')
	brokerInput!: HTMLSelectElement;

	@Ref('account')
	accountInput!: HTMLSelectElement;

	brokerId = '';
	accountId = '';

	get brokers() {
		const empty: BrokerDto[] = [{ displayName: '' }];
		const brokers = this.activeState.brokers;
		if (brokers) {
			return empty.concat(brokers);
		}
		return empty;
	}

	get accounts() {
		const empty: AccountDto[] = [{ displayName: '' }];
		const accounts = this.activeState.brokers?.find((b) => b.id == this.brokerId)?.accounts;
		if (accounts) {
			return empty.concat(accounts);
		}
		return empty;
	}

	onBrokerChange() {
		this.brokerId = this.brokerInput.value;
		this.accountId = '';
	}

	onAccountChange() {
		this.accountId = this.accountInput.value;
	}
}
</script>
