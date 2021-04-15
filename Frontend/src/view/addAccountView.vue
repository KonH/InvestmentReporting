<template>
	<h1>Add Account</h1>
	<div class="form-group">
		<label>
			Name:
			<input ref="displayName" type="text" class="form-control" />
		</label>
	</div>
	<div>
		<label>
			Currency:
			<select ref="currency" class="form-control">
				<option v-for="currency in currencies" :key="currency.id" :value="currency.id">{{ currency.code }} ({{ currency.format }})</option>
			</select>
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/config" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Action, State } from 'vuex-class';
import { StateDto } from '@/api/state';
import { Ref } from 'vue-property-decorator';

@Options({
	name: 'AddAccountView',
})
export default class AddAccount extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Ref('displayName')
	displayNameInput!: HTMLInputElement;

	@Ref('currency')
	currencySelect!: HTMLSelectElement;

	@Action('fetchState')
	fetchState!: () => void;

	get currencies() {
		return this.activeState.currencies;
	}

	async onclick() {
		const brokerId = this.$route.params.broker as string;
		const currencyId = this.currencySelect.value;
		const result = await Backend.tryFetch(
			Backend.state().account.accountCreate({
				broker: brokerId,
				currency: currencyId,
				displayName: this.displayNameInput.value,
			})
		);
		if (result?.ok) {
			this.fetchState();
			await router.push('/config');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
